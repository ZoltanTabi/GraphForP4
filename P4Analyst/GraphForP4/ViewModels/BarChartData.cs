using System.Collections.Generic;

namespace GraphForP4.ViewModels
{
    public class BarChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public Dictionary<string, List<double>> DoubleDatas { get; set; } = new Dictionary<string, List<double>>();
    }
}
