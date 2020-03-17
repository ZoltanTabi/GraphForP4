using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphForP4.Models
{
    public class Graph
    {
        public List<Node> Nodes { get; set; } = new List<Node>();

        public Node this[int i]
        {
            get => Nodes[i];
            set => Nodes[i] = value;
        }

        public Node this[string text]
        {
            get => Nodes.FirstOrDefault(x => x.Text == text);
        }

        public void Add(Node node)
        {
            this.Nodes.Add(node);
        }

        public void AddEdge(Node parent, Node child)
        {
            parent.Edges.Add(new Edge
            {
                Parent = parent,
                Child = child
            });
        }
    }
}
