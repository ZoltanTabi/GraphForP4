using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
            Nodes.Add(node);
        }

        public void AddEdge(Node parent, Node child, Color? color = null)
        {
            parent.Edges.Add(new Edge
            {
                Parent = parent,
                Child = child,
                Color = color.GetValueOrDefault(Color.Black)
            });
        }
    }
}
