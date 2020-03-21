using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForP4.ViewModels
{
    public class AngularNode
    {
        public Tuple<String, String> Node { get; set; }
        public Dictionary<Tuple<String,String>,String> Edges { get; set; }
        public int Number { get; set; }
    }
}
