using GraphForP4.Models;
using System.Collections.Generic;

namespace GraphForP4.ViewModels
{
    public class AnalyzeData
    {
        public int Id { get; set; }
        public List<Struct> StartState { get; set; }
        public List<Struct> EndState { get; set; }
    }
}
