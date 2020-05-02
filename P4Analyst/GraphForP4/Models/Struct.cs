using System.Collections.Generic;

namespace GraphForP4.Models
{
    public class Struct
    {
        public string Name { get; set; }
        public Dictionary<string, Struct> Structs { get; set; } = new Dictionary<string, Struct>();
        public Dictionary<string, Header> Headers { get; set; } = new Dictionary<string, Header>();
        public List<Variable> Variables { get; set; } = new List<Variable>();

        public object this[string key]
        {
            get
            {
                if (Structs.ContainsKey(key))
                {
                    return Structs[key];
                }
                if (Headers.ContainsKey(key))
                {
                    return Headers[key];
                }

                return Variables.Find(x => x.Name == key);
            }
        }
    }
}
