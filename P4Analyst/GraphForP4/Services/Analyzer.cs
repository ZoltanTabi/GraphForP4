using GraphForP4.Models;
using GraphForP4.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using GraphForP4.Helpers;
using GraphForP4.Extensions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Drawing;

namespace GraphForP4.Services
{
    public class Analyzer
    {
        private readonly string code;
        private readonly Graph controlFlowGraph;
        private readonly Graph dataFlowGraph;
        private readonly AnalyzeData analyzeData;
        private readonly AnalyzeData originalAnalyzeData;
        private readonly Dictionary<string, Struct> ingressEndStructs;
        private readonly List<Dictionary<string, Struct>> allStructs;

        public Analyzer(string controlFlowGrapJson, string dataFlowGraphJson, AnalyzeData analyzeData, string code)
        {
            controlFlowGraph = new Graph();
            controlFlowGraph.FromJson(controlFlowGrapJson);
            dataFlowGraph = new Graph();
            dataFlowGraph.FromJson(dataFlowGraphJson);
            this.analyzeData = analyzeData;
            originalAnalyzeData = Copy(analyzeData);
            ingressEndStructs = new Dictionary<string, Struct>();
            allStructs = new List<Dictionary<string, Struct>>();
            this.code = FileHelper.InputClean(code);
        }

        public void Analyze()
        {
            var ingressStartStructs = new Dictionary<string, Struct>();
            var ingressControlName = FileHelper.GetIngressControlName(code);
            Regex.Replace(FileHelper.GetMethod(code, ingressControlName, '(', ')'), @"(|)", String.Empty).Trim()
                .Split(",", StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(variable =>
                {
                    var declarations = variable.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
                    switch (declarations.Count)
                    {
                        case 2:
                            break;
                        case 3:
                            declarations.RemoveAt(0);
                            break;
                        default:
                            throw new ApplicationException("Érvénytelen változó átadás!");
                    }
                    var name = declarations[1].Trim();
                    var type = declarations[0].Trim();
                    var startStruct = analyzeData.StartState.FirstOrDefault(x => x.Name == type);
                    var endStruct = analyzeData.EndState.FirstOrDefault(x => x.Name == type);
                   
                    ingressStartStructs.Add(name, startStruct);
                    ingressEndStructs.Add(name, endStruct);
                });
            
            foreach (var edge in controlFlowGraph.Nodes[0].Edges)
            {
                NodeAnalyze(edge.Child, new List<Dictionary<string, Struct>> { Copy(ingressStartStructs) });
            }
            /*object v = new object();
            Struct _struct;
            Header header;
            Variable variable;
            switch(v.GetType().Name)
            {
                case "Struct":
                    _struct = (Struct)v;
                    break;
                case "Header":
                    header = (Header)v;
                    break;
                case "Variable":
                    variable = (Variable)v;
                    break;
            }*/
        }

        private void NodeAnalyze(Node node, List<Dictionary<string, Struct>> ingressStartStructsLists)
        {
            node.ModifiedAndUse = true;
            node.Modified = node.Modified == null ? 1 : ++node.Modified;
            var ifNodeWithValue = false;
            ingressStartStructsLists.ForEach(x =>
            {
                switch(node.Type)
                {
                    case NodeType.If:
                        ifNodeWithValue = IfAnalyze(node, x);
                        break;

                    case NodeType.Table:
                        TableAnalyze(node, x);
                        break;

                    case NodeType.ActionMethod:
                        ActionAnalyze(node, x);
                        break;
                }
            });

            if (!ifNodeWithValue)
            {
                foreach (var edge in node.Edges)
                {
                    if (node.Text != "End")
                    {
                        var toChild = new List<Dictionary<string, Struct>>();
                        ingressStartStructsLists.ForEach(x =>
                        {
                            toChild.Add(Copy(x));
                        });
                        NodeAnalyze(edge.Child, toChild);
                    }
                    else
                    {
                        allStructs.AddRange(ingressStartStructsLists);
                    }
                }
            }
        }

        private bool IfAnalyze(Node controlFlowGraphNode, Dictionary<string, Struct> ingressStartStruct)
        {
            var dataFlowGraphNode = dataFlowGraph.Nodes.Where(x => x.ParentId == controlFlowGraphNode.Id).ToList();
            if (dataFlowGraphNode.Count == 1 && dataFlowGraphNode[0].Text.Contains("isValid()"))
            {
                var header = GetHeaderFromStruct(ingressStartStruct, dataFlowGraphNode[0].Text.Split(".").ToList()).Item1;
                if (header != null)
                {
                    ++header.Use;
                    if (header.Valid)
                    {
                        controlFlowGraphNode.Edges.Where(x => x.Color == Color.Green).ToList().ForEach(x =>
                        {
                            NodeAnalyze(x.Child, new List<Dictionary<string, Struct>> { Copy(ingressStartStruct) });
                        });

                        return true;
                    }
                    else
                    {
                        controlFlowGraphNode.Edges.Where(x => x.Color == Color.Green).ToList().ForEach(x =>
                        {
                            NodeAnalyze(x.Child, new List<Dictionary<string, Struct>> { Copy(ingressStartStruct) });
                        });

                        return true;
                    }
                }
            }
            else
            {
                dataFlowGraphNode.ForEach(x =>
                {
                    Header header;
                    string variableName;
                    (header, variableName) = GetHeaderFromStruct(ingressStartStruct, x.Text.Split(".").ToList());
                    if (header != null)
                    {
                        ++header.Use;
                        var variable = header.Variables.FirstOrDefault(x => x.Name == variableName);
                        if (variable != null)
                        {
                            if (variable.IsInitialize)
                            {
                                ++variable.Read;
                                x.FillColor = Color.Green;
                            }
                            else
                            {
                                if (x.FillColor == Color.Green)
                                {
                                    x.FillColor = Color.Yellow;
                                }
                                else
                                {
                                    x.FillColor = Color.Red;
                                }
                            }
                        }
                    }
                });
            }
            return false;
        }

        private void TableAnalyze(Node controlFlowGraphNode, Dictionary<string, Struct> ingressStartStruct)
        {
            dataFlowGraph.Nodes.Where(x => x.ParentId == controlFlowGraphNode.Id).ToList().ForEach(x =>
            {
                Header header;
                string variableName;
                (header, variableName) = GetHeaderFromStruct(ingressStartStruct, x.Text.Split(".").ToList());
                if (header != null)
                {
                    ++header.Use;
                    var variable = header.Variables.FirstOrDefault(x => x.Name == variableName);
                    if (variable != null)
                    {
                        if (variable.IsInitialize)
                        {
                            ++variable.Read;
                            x.FillColor = Color.Green;
                        }
                        else
                        {
                            if (x.FillColor == Color.Green)
                            {
                                x.FillColor = Color.Yellow;
                            }
                            else
                            {
                                x.FillColor = Color.Red;
                            }
                        }
                    }
                }
            });
        }

        private void ActionAnalyze(Node controlFlowGraphNode, Dictionary<string, Struct> ingressStartStruct)
        {
            dataFlowGraph.Nodes.GroupBy(x => x.ParentId).ToList().ForEach(x =>
            {
                if (x.Key != null)
                {

                }
            });
        }

        private (Header, string) GetHeaderFromStruct(Dictionary<string, Struct> dics, List<String> stringList)
        {
            if (dics.ContainsKey(stringList[0]))
            {
                var _struct = dics[stringList[0]];
                var i = 1;
                while (i < stringList.Count && _struct != null && _struct.Structs.ContainsKey(stringList[i]))
                {
                    _struct = _struct.Structs[stringList[i]];
                }
                if (_struct != null && _struct.Headers.ContainsKey(stringList[i]))
                {
                    return (_struct.Headers[stringList[i]], stringList[i+1]);
                }
            }
            return (null, String.Empty);
        }

        private T Copy<T>(T someThing)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(someThing));
        }

        #region Struktúrák
        const string HEADER = "header ";
        const string STRUCT = "struct "; 

        public static List<Struct> GetStructs(string input)
        {
            var cleanInput = FileHelper.InputClean(input);

            List<Header> headers = new List<Header>();
            GetBlocks(cleanInput, HEADER).ForEach(block =>
            {
                var header = new Header
                {
                    Name = block.SubStringByEndChar(0, '{').Replace('{', ' ').Trim()
                };
                GetDeclarationBlock(block, header.Name).ForEach(variable =>
                {
                    var declaration = variable.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (declaration.Length != 2) throw new ApplicationException("Érvénytelen változó!");

                    header.Variables.Add(new Variable(declaration[0].Trim(), declaration[1].Trim()));
                });
                headers.Add(header);
            });

            List<Struct> structs = new List<Struct>();
            GetBlocks(cleanInput, STRUCT).ForEach(block =>
            {
                var _struct = new Struct
                {
                    Name = block.SubStringByEndChar(0, '{').Replace('{', ' ').Trim()
                };
                GetDeclarationBlock(block, _struct.Name).ForEach(variable =>
                {
                    var declaration = variable.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (declaration.Length != 2) throw new ApplicationException("Érvénytelen változó!");

                    var variableType = declaration[0].Trim();
                    if (structs.Any(x => x.Name == variableType))
                    {
                        var childStruct = structs.Find(x => x.Name == variableType);
                        _struct.Structs.Add(declaration[1].Trim(), childStruct);
                    }
                    else if (headers.Any(x => x.Name == variableType))
                    {
                        var header = headers.Find(x => x.Name == variableType);
                        _struct.Headers.Add(declaration[1].Trim(), header);
                    }
                    else
                    {
                        _struct.Variables.Add(new Variable(variableType, declaration[1].Trim()));
                    }
                });

                structs.Add(_struct);
            });

            return structs;
        }

        private static List<string> GetBlocks(string input, string key)
        {
            List<string> blocks = new List<string>();
            input.AllIndexesOf(key).ForEach(start =>
            {
                blocks.Add(input.SubStringByEndChar(start, '}').Replace(key, String.Empty).Trim());
            });

            return blocks;
        }

        private static List<String> GetDeclarationBlock(string block, string name)
        {
            return Regex.Replace(FileHelper.GetMethod(block, name), @"{|}", String.Empty).Trim().Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        #endregion
    }
}
