using Xunit;
using GraphForP4.Services;
using GraphForP4.Models;
using System.Collections.Generic;
using System.Text.Json;
using GraphForP4.ViewModels;
using GraphForP4.Extensions;
using GraphForP4.Enums;

namespace Test
{
    public class GraphTest
    {
        [Fact]
        public void ControlFlowGraphTest()
        {
            var content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\demo1.txt");
            var graph = P4ToGraph.ControlFlowGraph(ref content);

            Assert.Equal("Start", graph[0].Text);

            Assert.Equal("ipv4_da_lpm", graph[1].Text);
            Assert.Equal(NodeType.Table, graph[1].Type);

            Assert.Equal("set_l2ptr", graph[2].Text);
            Assert.Equal(NodeType.Action, graph[2].Type);

            Assert.Equal("my_drop", graph[3].Text);
            Assert.Equal(NodeType.Action, graph[3].Type);

            Assert.Equal("mac_da", graph[4].Text);
            Assert.Equal(NodeType.Table, graph[4].Type);

            Assert.Equal("set_bd_dmac_intf", graph[5].Text);
            Assert.Equal(NodeType.Action, graph[5].Type);

            Assert.Equal("my_drop", graph[6].Text);
            Assert.Equal(NodeType.Action, graph[6].Type);

            Assert.Equal("End", graph[7].Text);

            Assert.Equal(graph[0].Edges[0].Child, graph[1]);

            Assert.Equal(graph[1].Edges[0].Child, graph[2]);

            Assert.Equal(graph[1].Edges[1].Child, graph[3]);

            Assert.Equal(graph[2].Edges[0].Child, graph[4]);

            Assert.Equal(graph[3].Edges[0].Child, graph[4]);

            Assert.Equal(graph[4].Edges[0].Child, graph[5]);

            Assert.Equal(graph[4].Edges[1].Child, graph[6]);

            Assert.Equal(graph[5].Edges[0].Child, graph[7]);

            Assert.Equal(graph[6].Edges[0].Child, graph[7]);

        }

        [Fact]
        public void DataFlowGraphTest()
        {
            var content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\demo1.txt");
            var controlFlowgraph = P4ToGraph.ControlFlowGraph(ref content);
            var graph = P4ToGraph.DataFlowGraph(content, controlFlowgraph);

            Assert.Equal("Start", graph[0].Text);

            Assert.Equal("hdr.ipv4.dstAddr", graph[1].Text);
            Assert.Equal(NodeType.Key, graph[1].Type);

            Assert.Equal("meta.fwd_metadata.l2ptr", graph[2].Text);
            Assert.Equal(NodeType.ActionMethod, graph[2].Type);

            Assert.Equal("l2ptr", graph[3].Text);
            Assert.Equal(NodeType.ActionMethod, graph[3].Type);

            Assert.Equal("mark_to_drop(stdmeta)", graph[4].Text);
            Assert.Equal(NodeType.ActionMethod, graph[4].Type);

            Assert.Equal("meta.fwd_metadata.l2ptr", graph[5].Text);
            Assert.Equal(NodeType.Key, graph[5].Type);

            Assert.Equal("meta.fwd_metadata.out_bd", graph[6].Text);
            Assert.Equal(NodeType.ActionMethod, graph[6].Type);

            Assert.Equal("bd", graph[7].Text);
            Assert.Equal(NodeType.ActionMethod, graph[7].Type);

            Assert.Equal("hdr.ethernet.dstAddr", graph[8].Text);
            Assert.Equal(NodeType.ActionMethod, graph[8].Type);

            Assert.Equal("dmac", graph[9].Text);
            Assert.Equal(NodeType.ActionMethod, graph[9].Type);

            Assert.Equal("stdmeta.egress_spec", graph[10].Text);
            Assert.Equal(NodeType.ActionMethod, graph[10].Type);

            Assert.Equal("intf", graph[11].Text);
            Assert.Equal(NodeType.ActionMethod, graph[11].Type);

            Assert.Equal("hdr.ipv4.ttl", graph[12].Text);
            Assert.Equal(NodeType.ActionMethod, graph[12].Type);

            Assert.Equal("hdr.ipv4.ttl - 1", graph[13].Text);
            Assert.Equal(NodeType.ActionMethod, graph[13].Type);

            Assert.Equal("mark_to_drop(stdmeta)", graph[14].Text);
            Assert.Equal(NodeType.ActionMethod, graph[14].Type);

            Assert.Equal("End", graph[15].Text);
        }

        [Fact]
        public void GetStructsTest()
        {
            var content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\demo1.txt");

            var structs = Analyzer.GetStructs(content);

            Assert.Equal(3, structs.Count);
            Assert.Equal("fwd_metadata_t", structs[0].Name);
            Assert.Equal(2, structs[0].Variables.Count);
            Assert.Equal("l2ptr", structs[0].Variables[0].Name);
            Assert.Equal("bit<32>", structs[0].Variables[0].Type);
            Assert.Equal("out_bd", structs[0].Variables[1].Name);
            Assert.Equal("bit<24>", structs[0].Variables[1].Type);

            Assert.Equal(structs[0], structs[1].Structs["fwd_metadata"]);

            Assert.Equal(2, structs[2].Headers.Count);
            Assert.Equal("ethernet_t", structs[2].Headers["ethernet"].Name);
            Assert.Equal(3, structs[2].Headers["ethernet"].Variables.Count);
            Assert.Equal("dstAddr", structs[2].Headers["ethernet"].Variables[0].Name);
            Assert.Equal("srcAddr", structs[2].Headers["ethernet"].Variables[1].Name);
            Assert.Equal("etherType", structs[2].Headers["ethernet"].Variables[2].Name);

            Assert.Equal("ipv4_t", structs[2].Headers["ipv4"].Name);
            Assert.Equal("version", structs[2].Headers["ipv4"].Variables[0].Name);
            Assert.Equal("ihl", structs[2].Headers["ipv4"].Variables[1].Name);
            Assert.Equal("diffserv", structs[2].Headers["ipv4"].Variables[2].Name);
            Assert.Equal("totalLen", structs[2].Headers["ipv4"].Variables[3].Name);
            Assert.Equal("identification", structs[2].Headers["ipv4"].Variables[4].Name);
            Assert.Equal("flags", structs[2].Headers["ipv4"].Variables[5].Name);
            Assert.Equal("fragOffset", structs[2].Headers["ipv4"].Variables[6].Name);
            Assert.Equal("ttl", structs[2].Headers["ipv4"].Variables[7].Name);
            Assert.Equal("protocol", structs[2].Headers["ipv4"].Variables[8].Name);
            Assert.Equal("hdrChecksum", structs[2].Headers["ipv4"].Variables[9].Name);
            Assert.Equal("srcAddr", structs[2].Headers["ipv4"].Variables[10].Name);
            Assert.Equal("dstAddr", structs[2].Headers["ipv4"].Variables[11].Name);
        }

        [Fact]
        public void AnalyzeTest()
        {
            var content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\demo1.txt");

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

            var controlFlowGraph = analyzer.ControlFlowGraph;
            Assert.Equal(0, controlFlowGraph[0].Use);
            Assert.Equal(1, controlFlowGraph[1].Use);
            Assert.Equal(1, controlFlowGraph[2].Use);
            Assert.Equal(1, controlFlowGraph[3].Use);
            Assert.Equal(2, controlFlowGraph[4].Use);
            Assert.Equal(2, controlFlowGraph[5].Use);
            Assert.Equal(2, controlFlowGraph[6].Use);
            Assert.Equal(4, controlFlowGraph[7].Use);
        }
    }
}
