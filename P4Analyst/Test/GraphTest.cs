using System;
using Xunit;
using GraphForP4.Services;
using GraphForP4.Models;
using System.Collections.Generic;
using System.Text.Json;
using GraphForP4.ViewModels;

namespace Test
{
    public class GraphTest
    {
        [Fact]
        public void TestCreateGraph()
        {
            var content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\P4.txt");
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
        public void TestGetStructs()
        {
            var content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\demo7.txt");

            var structs = Analyzer.GetStructs(content);
            var graph = P4ToGraph.ControlFlowGraph(ref content);
            var graph1 = P4ToGraph.DataFlowGraph(content, graph);

            structs.ForEach(_struct =>
            {
                foreach (var header in _struct.Headers.Values)
                {
                    header.Valid = true;
                    header.Variables.ForEach(x => x.IsInitialize = true);
                }
            });

            var analyzeData = new AnalyzeData
            {
                EndState = structs,
                Id = 1,
                StartState = JsonSerializer.Deserialize<List<Struct>>(JsonSerializer.Serialize(structs))
            };

            var analyzer = new Analyzer(graph.ToJson(), graph1.ToJson(), analyzeData, content);
            analyzer.Analyze();
            analyzer.FinishOperations();


            Assert.Equal("ipv4_lpm", structs[0].Name);
        }
    }
}
