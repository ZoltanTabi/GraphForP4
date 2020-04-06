using System;
using System.Collections.Generic;
using System.Drawing;

namespace GraphForP4.Models
{
    public class Node
    {
        public string Text { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public IList<Edge> Edges { get; set; } = new List<Edge>();
        public NodeType Type { get; set; }
        public Color FillColor { get; set; } = Color.White;
        public Color FontColor { get; set; } = Color.Black;
        public NodeShape Shape { get; set; } = NodeShape.Ellipse;

        public override string ToString()
        {
            /*return "[fillcolor=\"#" + FillColor.R.ToString("X2") + FillColor.G.ToString("X2") + FillColor.B.ToString("X2")
                 + "\", fontcolor=\"#" + FontColor.R.ToString("X2") + FontColor.G.ToString("X2") + FontColor.B.ToString("X2")
                 + "\", shape=\"" + Shape.ToString("g").ToLower() + "\"]";*/
            return "[fillcolor=\"" + FillColor.Name.ToLower()
                 + "\", fontcolor=\"" + FontColor.Name.ToLower()
                 + "\", shape=\"" + Shape.ToString("g").ToLower() + "\"]";
        }
    }
}
