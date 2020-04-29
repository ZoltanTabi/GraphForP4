using System;
using System.Collections.Generic;
using System.Drawing;
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
            currentNodes.ForEach(node =>
            {
                var item = new AngularNode
                {
                    Text = node.Text,
                    Id = node.Id.ToString().Replace("-", String.Empty),
                    Number = level,
                    FillColor = $"#{node.FillColor.R:X2}{node.FillColor.G:X2}{node.FillColor.B:X2}",
                    FontColor = $"#{node.FontColor.R:X2}{node.FontColor.G:X2}{node.FontColor.B:X2}",
                    Shape = (int)node.Shape,
                    Tooltip = node.Tooltip,
                    ParentId = node.ParentId != null ? node.ParentId.ToString().Replace("-", String.Empty) : String.Empty,
                    SubGraph = node.SubGraph != null ? "cluster" + node.SubGraph.ToString().Replace("-", String.Empty) : String.Empty,
                    Edges = new List<AngularEdge>()
                };

                foreach (var edge in node.Edges)
                {
                    item.Edges.Add(new AngularEdge()
                    {
                        Parent = edge.Parent.Id.ToString().Replace("-", String.Empty),
                        Child = edge.Child.Id.ToString().Replace("-", String.Empty),
                        Color = $"#{edge.Color.R:X2}{edge.Color.G:X2}{edge.Color.B:X2}",
                        EdgeArrowType = (int)edge.EdgeArrowType,
                        EdgeStyle = (int)edge.EdgeStyle
                    });
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
            foreach (var angularNode in angularGraph)
            {
                for(var i = childNodes.Count - 1; i >= 0; --i)
                {
                    if (childNodes[i].Id.ToString().Replace("-", String.Empty) == angularNode.Id) childNodes.Remove(childNodes[i]);
                }
            }
        }
    }
}
