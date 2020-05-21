using System.Collections.Generic;

namespace GraphForP4.ViewModels
{
    public class PieChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Datas { get; set; } = new List<int>();
        public List<double> DoubleDatas { get; set; } = new List<double>();
    }
}
