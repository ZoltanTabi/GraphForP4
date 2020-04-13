using GraphForP4.Enums;
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
        public string Tooltip { get; set; }
        public NodeType Type { get; set; }
        public Color FillColor { get; set; } = Color.White;
        public Color FontColor { get; set; } = Color.Black;
        public NodeShape Shape { get; set; } = NodeShape.Ellipse;

        //DataFlowGraph Node
        public Guid? ParentId { get; set; }
        public Operation? Operation { get; set; }
        public int? Modified { get; set; }
        public bool? ModifiedAndUse { get; set; }


        public override string ToString()
        {
            return $"[label=\"{Text}\", "
                 + $"tooltip=\"{Tooltip}\", "
                 + $"fillcolor=\"#{FillColor.R:X2}{FillColor.G:X2}{FillColor.B:X2}\", "
                 + $"fontcolor=\"#{FontColor.R:X2}{FontColor.G:X2}{FontColor.B:X2}\", "
                 + $"shape=\"{Shape.ToString("g").ToLower()}\"]";
        }
    }
}
