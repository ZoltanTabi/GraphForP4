﻿using System.Collections.Generic;

namespace GraphForP4.ViewModels
{
    public class CalculatedData
    {
        public IEnumerable<IEnumerable<ViewNode>> ControlFlowGraphs { get; set; }
        public IEnumerable<IEnumerable<ViewNode>> DataFlowGraphs { get; set; }
        public BarChartData ReadAndWriteChartData { get; set; }
        public PieChartData UseVariable { get; set; }
        public PieChartData Useful { get; set; }
        public BarChartData Headers { get; set; }
        public FileData File { get; set; }
    }
}
