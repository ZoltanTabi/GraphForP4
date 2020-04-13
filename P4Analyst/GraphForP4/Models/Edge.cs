using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using GraphForP4.Enums;

namespace GraphForP4.Models
{
    public class Edge
    {
        public Node Parent { get; set; }
        public Node Child { get; set; }
        public Color Color { get; set; } = Color.Black;
        public EdgeArrowType EdgeArrowType { get; set; } = EdgeArrowType.Normal;
        public EdgeStyle EdgeStyle { get; set; } = EdgeStyle.None;

        public override string ToString()
        {
            return $"[color=\"#{Color.R:X2}{Color.G:X2}{Color.B:X2}\", "
                 + $"arrowType=\"{EdgeArrowType.ToString("g").ToLower()}\", "
                 + $"style=\"{EdgeStyle.ToString("g").ToLower()}\"]";
        }
    }
}
