namespace GraphForP4.Models
{
    public class Variable
    {
        public Variable() { }

        public Variable(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsInitialize { get; set; }

        // Elemzéshez
        public int Write { get; set; }
        public int Read { get; set; }
        public bool Modified { get; set; }
        public int ModifiedAndUse { get; set; }
    }
}
