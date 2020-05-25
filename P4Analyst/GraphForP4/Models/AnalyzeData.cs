using System.Collections.Generic;

namespace GraphForP4.Models
{
    public class AnalyzeData
    {
        public int Id { get; set; }
        public List<Struct> StartState { get; set; }
        public List<Struct> EndState { get; set; }

    }
}
