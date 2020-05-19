using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForP4.ViewModels
{
    public class CalculatedData
    {
        public IEnumerable<IEnumerable<AngularNode>> ControlFlowGraphs { get; set; }
        public IEnumerable<IEnumerable<AngularNode>> DataFlowGraphs { get; set; }
        public BarChartData BarChartData { get; set; }
    }
}
