using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GraphForP4.Models
{
    public class Edge
    {
        public Node Parent { get; set; }
        public Node Child { get; set; }
        public Color Color { get; set; } = Color.Black;

        public override string ToString()
        {
            return "[color=\"#" + Color.R.ToString("X2") + Color.G.ToString("X2") + Color.B.ToString("X2") + "\"]";
        }
    }
}
