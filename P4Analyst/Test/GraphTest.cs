using System;
using Xunit;
using GraphForP4.Services;

namespace Test
{
    public class GraphTest
    {
        [Fact]
        public void TestCreateGraph()
        {
            var content = System.IO.File.ReadAllText(@"C:\Users\Tábi Zoltán\Desktop\Egyetem\Szakdolgozat\GraphForP4\P4Analyst\AngularApp\Files\demo7.txt");
            var graph = P4ToGraph.ControlFlowGraph(ref content);
            //var json = graph.ToJson();
            var graph1 = P4ToGraph.DataFlowGraph(content, graph);

            var angularGraph = GraphToAngular.Serialize(graph);
            var angularGraph1 = GraphToAngular.Serialize(graph1);

            Assert.Equal("ipv4_lpm", graph[0].Text);
            Assert.Equal("ipv4_forward", graph[1].Text);
            Assert.Equal("drop", graph[2].Text);
            Assert.Equal("NoAction", graph[3].Text);

            Assert.Equal(graph[0].Edges[0].Child, graph[1]);
            Assert.Equal(graph[0].Edges[1].Child, graph[2]);
            Assert.Equal(graph[0].Edges[2].Child, graph[3]);
        }

        [Fact]
        public void TestGetVariables()
        {
            var content = System.IO.File.ReadAllText(@"C:\Users\Tábi Zoltán\Desktop\Egyetem\Szakdolgozat\GraphForP4\P4Analyst\AngularApp\Files\hello.txt");

            var variables = Analyzer.GetStructs(content);

            Assert.Equal("ipv4_lpm", variables[0].Name);
        }
    }
}
