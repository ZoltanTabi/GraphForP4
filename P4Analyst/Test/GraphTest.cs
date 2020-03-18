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
            var graph = P4ToGraph.Create(System.IO.File.ReadAllText(@"C:\Users\T�bi Zolt�n\Desktop\hello.txt"));

            var angularGraph = GraphToAngular.Serialize(graph);

            Assert.Equal("ipv4_lpm", graph[0].Text);
            Assert.Equal("ipv4_forward", graph[1].Text);
            Assert.Equal("drop", graph[2].Text);
            Assert.Equal("NoAction", graph[3].Text);

            Assert.Equal(graph[0].Edges[0].Child, graph[1]);
            Assert.Equal(graph[0].Edges[1].Child, graph[2]);
            Assert.Equal(graph[0].Edges[2].Child, graph[3]);
        }
    }
}
