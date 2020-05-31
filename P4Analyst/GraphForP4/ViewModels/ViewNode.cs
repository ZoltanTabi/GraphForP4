using System.Collections.Generic;

namespace GraphForP4.ViewModels
{
    public class ViewNode
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public string Text { get; set; }
        public string ParentId { get; set; }
        public string SubGraph { get; set; }
        public string Tooltip { get; set; }
        public List<ViewEdge> Edges { get; set; }
        public string FillColor { get; set; }
        public string FontColor { get; set; }
        public int Shape { get; set; }
    }
}
