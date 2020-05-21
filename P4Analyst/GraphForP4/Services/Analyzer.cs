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
        private readonly AnalyzeData analyzeData;
        private readonly Dictionary<string, Struct> ingressEndStructs;
        
        public Graph ControlFlowGraph { get; set; }
        public Graph DataFlowGraph { get; set; }
        public List<Dictionary<string, Struct>> AllStructs { get; set; }
        public int Id { get { return analyzeData.Id; } }

        public Analyzer(string controlFlowGrapJson, string dataFlowGraphJson, AnalyzeData analyzeData, string code)
        {
            ControlFlowGraph = new Graph();
            ControlFlowGraph.FromJson(controlFlowGrapJson);
            DataFlowGraph = new Graph();
            DataFlowGraph.FromJson(dataFlowGraphJson);
            this.analyzeData = analyzeData;
            ingressEndStructs = new Dictionary<string, Struct>();
            AllStructs = new List<Dictionary<string, Struct>>();
            this.code = FileHelper.InputClean(code);
        }

        public void Analyze()
        {
            var ingressStartStructs = new Dictionary<string, Struct>();
            var ingressControlName = FileHelper.GetIngressControlName(code);
            Regex.Replace(FileHelper.GetMethod(code, ingressControlName, '(', ')'), @"\(|\)", String.Empty).Trim()
                .Split(",", StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(variable =>
                {
                    var declarations = variable.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
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
            
            foreach (var edge in ControlFlowGraph.Nodes[0].Edges)
            {
                edge.Color = Color.Green;
                NodeAnalyze(edge.Child, new List<Dictionary<string, Struct>> { Copy(ingressStartStructs) });
            }
        }

        private void NodeAnalyze(Node node, List<Dictionary<string, Struct>> ingressStartStructsLists)
        {
            node.ModifiedAndUse = true;
            node.Modified = node.Modified == null ? 1 : ++node.Modified;
            var correct = true;
            var addAllStruct = true;
            ingressStartStructsLists.ForEach(x =>
            {
                switch(node.Type)
                {
                    case NodeType.If:
                        (correct, addAllStruct) = IfAnalyze(node, x);
                        break;

                    case NodeType.Table:
                        correct = TableAnalyze(node, x);
                        break;

                    case NodeType.ActionMethod:
                        var dataFlowGraphNodes = DataFlowGraph.Nodes.Where(y => y.ParentId == node.Id).ToList();
                        
                        correct = ActionAnalyze(dataFlowGraphNodes, x, MainNodes(dataFlowGraphNodes).FirstOrDefault());
                        break;

                    default:
                        node.FillColor = Color.Green;
                        break;
                }
            });

            if (correct)
            {
                node.FillColor = Color.Green;
            }

            if (correct && node.Text != "End")
            {
                foreach (var edge in node.Edges)
                {
                    edge.Color = Color.Green;
                    var toChild = new List<Dictionary<string, Struct>>();
                    ingressStartStructsLists.ForEach(x =>
                    {
                        toChild.Add(Copy(x));
                    });
                    NodeAnalyze(edge.Child, toChild);
                }
            }
            else if(addAllStruct)
            {
                AllStructs.AddRange(ingressStartStructsLists);
            }
        }

        private (bool, bool) IfAnalyze(Node controlFlowGraphNode, Dictionary<string, Struct> ingressStartStruct)
        {
            var dataFlowGraphNode = DataFlowGraph.Nodes.Where(x => x.ParentId == controlFlowGraphNode.Id).ToList();
            if (dataFlowGraphNode.Count == 1 && dataFlowGraphNode[0].Text.Contains("isValid()"))
            {
                dataFlowGraphNode[0].FillColor = Color.Green;
                var header = GetHeaderFromStruct(ingressStartStruct, dataFlowGraphNode[0].Text.Split(".").ToList()).Item1;
                if (header != null)
                {
                    ++header.Use;
                    if (header.Valid)
                    {
                        controlFlowGraphNode.Edges.Where(x => x.Color == Color.Red).ToList().ForEach(x =>
                        {
                            x.Color = Color.Black;
                        });

                        controlFlowGraphNode.Edges.Where(x => x.Color == Color.Green).ToList().ForEach(x =>
                        {
                            NodeAnalyze(x.Child, new List<Dictionary<string, Struct>> { Copy(ingressStartStruct) });
                        });

                        return (false, false);
                    }
                    else
                    {
                        controlFlowGraphNode.Edges.Where(x => x.Color == Color.Green).ToList().ForEach(x =>
                        {
                            x.Color = Color.Black;
                        });

                        controlFlowGraphNode.Edges.Where(x => x.Color == Color.Red).ToList().ForEach(x =>
                        {
                            x.Color = Color.Green;
                            NodeAnalyze(x.Child, new List<Dictionary<string, Struct>> { Copy(ingressStartStruct) });
                        });

                        return (false, false);
                    }
                }
            }
            else
            {
                var result = true;
                dataFlowGraphNode.ForEach(x =>
                {
                    result = result && UseVariable(x, ingressStartStruct);
                });

                return (result, true);
            }

            return (true, true);
        }

        private bool TableAnalyze(Node controlFlowGraphNode, Dictionary<string, Struct> ingressStartStruct)
        {
            var result = true;
            DataFlowGraph.Nodes.Where(x => x.ParentId == controlFlowGraphNode.Id).ToList().ForEach(x =>
            {
                result = result && UseVariable(x, ingressStartStruct);
            });

            return result;
        }

        private bool ActionAnalyze(List<Node> dataFlowGraphNodes, Dictionary<string, Struct> ingressStartStruct, Node node)
        {
            if (node.SubGraph == null)
            {
                node.FillColor = Color.Green;
                if (node.Text.Contains("valid", StringComparison.OrdinalIgnoreCase))
                {
                    Header header;
                    string command;
                    (header, command) = GetHeaderFromStruct(ingressStartStruct, node.Text.Split(".", StringSplitOptions.RemoveEmptyEntries).ToList());
                    command =  Regex.Replace(command, @"\(|\)|\;|,", String.Empty).Trim();
                    if (header != null && (command.Equals("setinvalid", StringComparison.OrdinalIgnoreCase) || command.Equals("setvalid", StringComparison.OrdinalIgnoreCase)))
                    {
                        ++header.Use;
                        header.Valid = command.Equals("setvalid", StringComparison.OrdinalIgnoreCase);
                        header.Variables.ForEach(variable =>
                        {
                            variable.IsInitialize = false;
                        });
                    }
                }
            }
            else
            {
                var actionNodes = dataFlowGraphNodes.Where(x => x.SubGraph == node.SubGraph).ToList();

                var correct = true;
                var readNode = actionNodes.FirstOrDefault(x => x.Operation == Operation.Read);

                Regex.Split(readNode.Text, @"\+|\-|\*|\/").ToList().ForEach(x =>
                {
                    correct = correct && UseVariable(new Node { Text = x }, ingressStartStruct);
                });

                BrushNode(readNode, correct);

                if (!correct)
                {
                    return false;
                }

                if (!UseVariable(actionNodes.FirstOrDefault(x => x.Operation == Operation.Write), ingressStartStruct, false))
                {
                    return false;
                }

                node = readNode;
            }


            if (dataFlowGraphNodes.Contains(node.Edges.FirstOrDefault().Child))
            {
                ActionAnalyze(dataFlowGraphNodes, ingressStartStruct, node.Edges.FirstOrDefault().Child);
            }

            return true;
        }

        private (Header, string) GetHeaderFromStruct(Dictionary<string, Struct> dics, List<String> stringList)
        {
            if (stringList.Any() && dics.ContainsKey(stringList[0]))
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

        private bool UseVariable (Node node, Dictionary<string, Struct> ingressStartStruct, bool isRead = true)
        {
            Header header;
            string variableName;
            (header, variableName) = GetHeaderFromStruct(ingressStartStruct, node.Text.Split(".", StringSplitOptions.RemoveEmptyEntries).ToList());
            if (header != null)
            {
                ++header.Use;
                var variable = header.Variables.FirstOrDefault(x => x.Name == variableName);
                if (variable != null)
                {
                    if (isRead)
                    {
                        ++variable.Read;
                        if (variable.Modified)
                        {
                            ++variable.ModifiedAndUse;
                            variable.Modified = false;
                        }
                    }
                    else
                    {
                        ++variable.Write;
                        variable.Modified = true;
                        variable.IsInitialize = true;
                    }

                    BrushNode(node, variable.IsInitialize);

                    return isRead ? variable.IsInitialize : true;
                }
            }
            else
            {
                node.FillColor = Color.Green;
            }

            return true;
        }

        private void BrushNode(Node node, bool correct)
        {
            if (node.FillColor == Color.White)
            {
                node.FillColor = correct ? Color.Green : Color.Red;
            }
            else if ((node.FillColor == Color.Red && correct) || (node.FillColor == Color.Green && correct))
            {
                node.FillColor = Color.Yellow;
            }
        }

        private static List<Node> MainNodes(List<Node> nodes)
        {
            var notMainNodes = new List<Node>();
            nodes.ForEach((node) =>
            {
                foreach (var otherNode in nodes)
                {
                    foreach (var edge in otherNode.Edges)
                    {
                        if (edge.Child == node) notMainNodes.Add(node);
                    }
                }
            });

            return nodes.Except(notMainNodes).ToList();
        }

        public void FinishOperations()
        {
            AllStructs.ForEach(dict =>
            {
                foreach(var pair in dict)
                {
                    if (pair.Value == null) continue;

                    var endStruct = ingressEndStructs[pair.Key];
                    foreach(var headerPair in pair.Value.Headers)
                    {
                        var headerForEndStruct = endStruct.Headers[headerPair.Key];
                        headerPair.Value.Variables.ForEach(headerPairVariable =>
                        {
                            headerForEndStruct.Variables.ForEach(headerForEndStructVariable =>
                            { 
                                if (headerPairVariable.Modified && headerForEndStructVariable.IsInitialize)
                                {
                                    headerPairVariable.Modified = false;
                                    ++headerPairVariable.ModifiedAndUse;
                                }
                            });
                        });
                    }
                }
            });

            ControlFlowGraph.Nodes.ForEach(node =>
            {
                var dataFlowGraphNodes = DataFlowGraph.Nodes.Where(x => x.ParentId == node.Id).ToList();
                if (dataFlowGraphNodes.Count == 1)
                {
                    node.FillColor = dataFlowGraphNodes.First().FillColor;
                }
                else if (dataFlowGraphNodes.Any())
                {
                    var colors = dataFlowGraphNodes.Select(x => x.FillColor).Distinct().Except(new List<Color> { Color.Black, Color.White }).ToList();
                    if (colors.Count == 1)
                    {
                        node.FillColor = colors.First();
                    }
                    else if (colors.Any())
                    {
                        node.FillColor = Color.Yellow;
                    }
                }
            });
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
