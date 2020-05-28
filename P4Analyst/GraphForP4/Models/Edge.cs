using System.Drawing;
using GraphForP4.Enums;

namespace GraphForP4.Models
{
    public class Edge
    {
        public Node Parent { get; set; }
        public Node Child { get; set; }
        public Color Color { get; set; }
        public EdgeArrowType ArrowType { get; set; } = EdgeArrowType.Normal;
        public EdgeStyle Style { get; set; } = EdgeStyle.None;
    }
}
