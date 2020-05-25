using System.Collections.Generic;

namespace GraphForP4.Models
{
    public class Struct
    {
        public string Name { get; set; }
        public Dictionary<string, Struct> Structs { get; set; } = new Dictionary<string, Struct>();
        public Dictionary<string, Header> Headers { get; set; } = new Dictionary<string, Header>();
        public List<Variable> Variables { get; set; } = new List<Variable>();
    }
}
