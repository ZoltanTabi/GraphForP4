using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForP4.ViewModels
{
    public class AngularEdge
    {
        public string Parent { get; set; }
        public string Child { get; set; }
        public string Color { get; set; }
        public int EdgeArrowType { get; set; }
        public int EdgeStyle { get; set; }
    }
}
