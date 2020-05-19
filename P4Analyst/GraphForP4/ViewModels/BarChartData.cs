using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForP4.ViewModels
{
    public class BarChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<double> Reads { get; set; } = new List<double>();
        public List<double> Writes { get; set; } = new List<double>();
    }
}
