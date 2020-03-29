using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphForP4.Models;
using GraphForP4.ViewModels;

namespace GraphForP4.Services
{
    public static class GraphToAngular
    {
        public static IEnumerable<AngularNode> Serialize(Graph graph)
        {
            var angularGraph = new List<AngularNode>();

            var level = 0;
            var currentNodes = new List<Node>();

            Parallel.ForEach(graph.Nodes, (node) =>
            {
                if (MainNode(graph, node)) currentNodes.Add(node);
                node.Text = node.Text.Insert(0, "\"") + "\"";
            });

            while (currentNodes.Any())
            {
                (currentNodes, level) = GenerateLevel(angularGraph, currentNodes, level);
            }

            return angularGraph;
        }

        private static (List<Node>, int) GenerateLevel(List<AngularNode> angularGraph, List<Node> currentNodes, int level)
        {
            var childNodes = new List<Node>();
            Parallel.ForEach(currentNodes, (node) =>
            {
                var item = new AngularNode
                {
                    Number = level,
                    Node = new Tuple<string, string>(node.Text, node.ToString()),
                    Edges = new List<Tuple<Tuple<string, string>, string>>()
                };
                foreach(var edge in node.Edges)
                {
                    item.Edges.Add(new Tuple<Tuple<string, string>, string>
                                    (new Tuple<string, string>(edge.Parent.Text, edge.Child.Text), edge.ToString())
                                  );
                    childNodes.Add(edge.Child);
                }
                angularGraph.Add(item);
            });

            CheckExisting(childNodes, angularGraph);

            return (childNodes.Distinct().ToList(), ++level);
        }

        private static bool MainNode(Graph graph, Node node)
        {
            var find = false;

            for(var i = 0; i < graph.Nodes.Count && !find; ++i)
            {
                for(var j = 0; j < graph.Nodes[i].Edges.Count && !find; ++j)
                {
                    find = graph.Nodes[i].Edges[j].Child == node;
                }
            }

            return !find;
        }

        private static void CheckExisting(List<Node> childNodes, List<AngularNode> angularGraph)
        {
            foreach(var angularNode in angularGraph)
            {
                for(var i = childNodes.Count - 1; i >= 0; --i)
                {
                    if (childNodes[i].Text == angularNode.Node.Item1) childNodes.Remove(childNodes[i]);
                }
            }
        }
    }
}
