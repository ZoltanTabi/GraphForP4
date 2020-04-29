using System.Collections.Generic;

namespace GraphForP4.Models
{
    public class Header
    {
        public string Name { get; set; }
        public List<Variable> Variables { get; set; } = new List<Variable>();
        public bool Valid { get; set; }
    }
}
