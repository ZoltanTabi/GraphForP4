using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphForP4.Enums;
using GraphForP4.Models;
using GraphForP4.Helpers;

namespace GraphForP4.Services
{
    public static class P4ToGraph
    {
        const string APPLY = "apply";
        const string IF = "if";
        const string ELSE = "else";
        const string ACTIONS = "actions";

        #region ControlFlowGraph
        public static Graph ControlFlowGraph(ref string input)
        {
            Graph graph = new Graph();
            graph.Add(new Node
            {
                FillColor = Color.Green,
                Text = "Start",
                Shape = NodeShape.Diamond,
                Tooltip = "Start"
            });
            List<Node> currentNodes = new List<Node>
            {
                graph[0]
            };

            input = FileHelper.InputClean(input);
            var ingressControlName = FileHelper.GetIngressControlName(input);

            var ingressMethod = FileHelper.GetMethod(input, ingressControlName);
            var applyMethod = FileHelper.SplitAndClean(FileHelper.GetMethod(ingressMethod, APPLY));

            currentNodes = Search(graph, currentNodes, applyMethod, ingressMethod, null);

            graph.Add(new Node
            {
                FillColor = Color.Red,
                Text = "End",
                Shape = NodeShape.Diamond,
                Tooltip = "End"
            });

            foreach(var node in currentNodes)
            {
                graph.AddEdge(node, graph["End"]);
            }

            for(var i = graph.Nodes.Count - 1; i >= 0; --i)
            {
                for(var j = graph.Nodes[i].Edges.Count -1; j >= 0; --j)
                {
                    if(graph.Nodes[i].Edges[j].Child.Type == NodeType.Skip)
                    {
                        for(var k = graph.Nodes[i].Edges[j].Child.Edges.Count - 1; k >= 0; --k)
                        {
                            graph.AddEdge(graph.Nodes[i].Edges[j].Parent, graph.Nodes[i].Edges[j].Child.Edges[k].Child, Color.Red);
                        }
                    }
                }
            }

            graph.Nodes.RemoveAll(new Predicate<Node>(x => x.Type == NodeType.Skip));
            foreach(var node in graph.Nodes)
            {
                for(var i = node.Edges.Count - 1; i >= 0; --i)
                {
                    if(node.Edges[i].Child.Type == NodeType.Skip)
                    {
                        node.Edges.RemoveAt(i);
                    }
                }
            }

            return graph;
        }

        private static List<Node> Search(Graph graph, List<Node> currentNodes, List<String> method, String ingressMethod, Color? edgeColor)
        {
            var current = String.Empty;
            (method, current) = Pop(method);
            while (!String.IsNullOrWhiteSpace(current))
            {
                if(current != IF)
                {
                    if (current.Contains(APPLY))
                    {
                        currentNodes = TableMethod(graph, currentNodes,
                                       Regex.Replace(current, @"\. *" + APPLY + @" *\( *\);", String.Empty).Trim(),
                                       ingressMethod, ref edgeColor);
                    }
                    else if (current.Contains('('))
                    {
                        currentNodes = ActionMethod(graph, currentNodes,
                                       Regex.Replace(current, @" *\(.*\);", String.Empty).Trim(),
                                       ingressMethod, ref edgeColor);
                    }
                    else
                    {
                        string actionMethod = current;
                        while (!String.IsNullOrWhiteSpace(current) && !current.Contains(';'))
                        {
                            (method, current) = Pop(method);
                            actionMethod += current;
                        }
                        var actionMethodNode = new Node()
                        {
                            Text = actionMethod,
                            Type = NodeType.ActionMethod,
                            Tooltip = actionMethod
                        };

                        graph.Add(actionMethodNode);
                        foreach (var node in currentNodes)
                        {
                            graph.AddEdge(node, actionMethodNode, edgeColor);
                        }
                        edgeColor = null;

                        currentNodes = new List<Node> { actionMethodNode };
                    }
                }
                else
                {
                    var ifMethod = current;

                    while((ifMethod.Count(x => x == '(') != ifMethod.Count(x => x == ')') || ifMethod.Count(x => x == '(') == 0)
                        || (ifMethod.Count(x => x == '{') != ifMethod.Count(x => x == '}') || ifMethod.Count(x => x == '}') == 0))
                    {
                        (method, current) = Pop(method);
                        ifMethod += " " + current;
                    }

                    var elseMethod = String.Empty;
                    if(method.Any() && method.FirstOrDefault().Contains(ELSE))
                    {
                        while (elseMethod.Count(x => x == '{') != elseMethod.Count(x => x == '}') || elseMethod.Count(x => x == '}') == 0)
                        {
                            (method, current) = Pop(method);
                            elseMethod += " " + current;
                        }
                    }

                    currentNodes = IfMethod(graph, currentNodes, ifMethod + elseMethod, ingressMethod, ref edgeColor);
                }
                (method, current) = Pop(method);
            }

            return currentNodes;
        }

        private static List<Node> IfMethod(Graph graph, List<Node> currentNodes, String ifMethod, String ingressMethod, ref Color? edgeColor)
        {
            var ifCondition = FileHelper.GetMethod(ifMethod, IF, '(', ')');
            ifCondition = ifCondition.Insert(0, $"{IF} ");
            var ifNode = new Node
            {
                Text = ifCondition,
                Type = NodeType.If,
                Tooltip = "if"
            };

            graph.Add(ifNode);

            foreach(var node in currentNodes)
            {
                graph.AddEdge(node, ifNode, edgeColor);
            }
            edgeColor = null;

            var ifTrueMethod = FileHelper.GetMethod(ifMethod, ifCondition);
            ifMethod = new string(ifMethod.Skip(ifCondition.Length).ToArray()).Trim().Replace(ifTrueMethod, String.Empty);
            currentNodes = Search(graph, new List<Node> { ifNode }, FileHelper.SplitAndClean(ifTrueMethod), ingressMethod, Color.Green);

            if(ifMethod.Contains(ELSE))
            {
                var elseMethod = FileHelper.SplitAndClean(FileHelper.GetMethod(ifMethod, ELSE));
                currentNodes.AddRange(Search(graph, new List<Node> { ifNode }, elseMethod, ingressMethod, Color.Red));
            }
            else
            {
                var skipNode = new Node
                {
                    Text = "SKIP",
                    Type = NodeType.Skip
                };
                graph.Add(skipNode);
                graph.AddEdge(ifNode, skipNode);
                currentNodes.Add(skipNode);
            }

            return currentNodes;
        }

        private static List<Node> TableMethod(Graph graph, List<Node> currentNodes, String tableName, String ingressMethod,  ref Color? edgeColor)
        {
            var tableNode = new Node
            {
                Text = tableName,
                Type = NodeType.Table,
                Tooltip = "Table"
            };

            graph.Add(tableNode);
            
            foreach(var node in currentNodes)
            {
                graph.AddEdge(node, tableNode, edgeColor);
            }
            edgeColor = null;

            currentNodes = new List<Node>();

            var tableMethod = FileHelper.GetMethod(ingressMethod, "table " + tableName);
            var actions = FileHelper.SplitAndClean(FileHelper.GetMethod(tableMethod, ACTIONS));
            foreach(var action in actions)
            {
                if(action.Contains(";"))
                {
                    currentNodes.AddRange(ActionMethod(graph, new List<Node> { tableNode },
                                   Regex.Replace(action, @"( |;)", String.Empty).Trim(), ingressMethod, ref edgeColor));
                }
            }

            return currentNodes;
        }

        private static List<Node> ActionMethod(Graph graph, List<Node> currentNodes, String actionName, String ingressMethod, ref Color? edgeColor)
        {
            var actionNode = new Node
            {
                Text = actionName,
                Type = NodeType.Action,
                Tooltip = "Action"
            };

            graph.Add(actionNode);
            foreach(var node in currentNodes)
            {
                graph.AddEdge(node, actionNode, edgeColor);
            }
            edgeColor = null;

            var actionMethod = FileHelper.GetMethod(ingressMethod, "action " + actionName).Trim();
            actionMethod = Regex.Replace(actionMethod, @" +", " ");
            
            if(String.IsNullOrWhiteSpace(actionMethod))
            {
                return new List<Node> { actionNode };
            }

            var actionMethodNode = new Node
            {
                Text = actionMethod,
                Type = NodeType.ActionMethod,
                Tooltip = actionMethod
            };

            graph.Add(actionMethodNode);
            graph.AddEdge(actionNode, actionMethodNode);

            return new List<Node> { actionMethodNode };
        }
        #endregion

        #region DataFlowGraph
        public static Graph DataFlowGraph(string input, Graph controlFlowGraph)
        {
            var graphs = new Dictionary<Guid, Graph>();
            var startNode = new Node()
            {
                Text = "Start",
                FillColor = Color.Green,
                Tooltip = "Start",
                Shape = NodeShape.Diamond
            };

            var graph = new Graph();
            graph.Add(startNode);

            foreach(var node in controlFlowGraph.Nodes)
            {
                switch (node.Type)
                {
                    case NodeType.If:
                        graphs.Add(node.Id, IfNode(node, graph));
                        break;

                    case NodeType.Table:
                        var tableGraph = TableNode(node, graph, input);
                        if(tableGraph.Nodes.Any())
                        {
                            graphs.Add(node.Id, tableGraph);
                        }
                        break;

                    case NodeType.ActionMethod:
                        graphs.Add(node.Id, ActionMethodNode(node, graph));
                        break;
                }
            }

            MainNodes(graphs.First().Value).ForEach(fromStartNode =>
            {
                graph.AddEdge(startNode, fromStartNode);
            });

            var queue = new Queue<Node>();
            foreach(var edge in controlFlowGraph[0].Edges)
            {
                queue.Enqueue(edge.Child);
            }

            while (queue.Any())
            {
                var node = queue.Dequeue();
                node.FillColor = Color.Gray;

                BFSHelper(node, node, graph, graphs, ref queue);

                node.FillColor = Color.Black;
            }

            var endNode = new Node()
            {
                Text = "End",
                FillColor = Color.Red,
                Tooltip = "End",
                Shape = NodeShape.Diamond
            };
            graph.Add(endNode);

            foreach(var controlFlowGraphNode in controlFlowGraph.Nodes)
            {
                var edge = controlFlowGraphNode.Edges.FirstOrDefault(x => x.Child == controlFlowGraph["End"]);
                if (graphs.ContainsKey(controlFlowGraphNode.Id) && edge != null)
                {
                    EndNodes(graphs[controlFlowGraphNode.Id]).ForEach(node =>
                    {
                        graph.AddEdge(node, endNode, edge.Color);
                    });
                }
            }

            return graph;
        }

        private static Graph IfNode(Node parentNode, Graph dataFlowGraph)
        {
            var graph = new Graph();

            var condition = FileHelper.GetMethod(parentNode.Text, IF, '(', ')');
            condition = condition.Remove(condition.Length - 1, 1).Remove(0, 1).Trim();

            var split = Regex.Split(condition, @"&&|\|\||==|!=").ToList();
            split = split.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            foreach (var text in split)
            {
                var node = new Node()
                {
                    ModifiedAndUse = false,
                    Modified = 0,
                    Operation = Operation.Read,
                    ParentId = parentNode.Id,
                    Text = text.Trim(),
                    Tooltip = "if",
                    Type = NodeType.If,
                    Shape = NodeShape.Box
                };
                graph.Add(node);
                dataFlowGraph.Add(node);
            }

            var nodes = new List<Node>();
            foreach(var node in graph.Nodes)
            {
                nodes.Add(node);
                foreach(var otherNode in graph.Nodes.Except(nodes))
                {
                    node.Edges.Add(new Edge()
                    {
                        Parent = node,
                        Child = otherNode,
                        EdgeArrowType = EdgeArrowType.None,
                        EdgeStyle = EdgeStyle.Dotted
                    });
                }
            }

            return graph;
        }

        private static Graph TableNode(Node parentNode, Graph dataFlowGraph, string input)
        {
            var graph = new Graph();

            var tableMethod = FileHelper.GetMethod(input, "table " + parentNode.Text);

            if (!tableMethod.Contains("key")) return graph;

            var keys = FileHelper.GetMethod(tableMethod, "key");

            if (keys.Trim().Length == 0) return graph;

            keys = keys.Remove(keys.Length - 1, 1).Remove(0, 1).Trim();

            Node previousNode = null;

            foreach (var dir in keys.Split(";", StringSplitOptions.RemoveEmptyEntries))
            {
                var key = dir.Split(":", StringSplitOptions.RemoveEmptyEntries)[0];

                if(key.Trim().Length > 0)
                {
                    var node = new Node()
                    {
                        ModifiedAndUse = false,
                        Modified = 0,
                        Operation = Operation.Read,
                        ParentId = parentNode.Id,
                        Text = key.Trim(),
                        Tooltip = "Key",
                        Type = NodeType.Key,
                        Shape = NodeShape.Box
                    };
                    graph.Add(node);
                    dataFlowGraph.Add(node);

                    if(previousNode != null)
                    {
                        graph.AddEdge(previousNode, node);
                    }

                    previousNode = node;
                }
            }

            return graph;
        }

        private static Graph ActionMethodNode(Node parentNode, Graph dataFlowGraph)
        {
            var graph = new Graph();

            Node previousNode = null;

            foreach (var line in parentNode.Text.Split(";", StringSplitOptions.RemoveEmptyEntries))
            {
                var newLine = Regex.Replace(line, @"{|}", String.Empty).Trim();

                if (newLine == String.Empty) continue;

                if (!line.Contains('='))
                {
                    var node = new Node()
                    {
                        ModifiedAndUse = false,
                        Modified = 0,
                        Operation = Operation.Read,
                        ParentId = parentNode.Id,
                        Text = newLine,
                        Tooltip = newLine,
                        Type = NodeType.ActionMethod,
                        Shape = NodeShape.Box
                    };

                    graph.Add(node);
                    dataFlowGraph.Add(node);

                    if (previousNode != null)
                    {
                        graph.AddEdge(previousNode, node);
                    }

                    previousNode = node;

                    continue;
                }

                var tokens = newLine.Split("=", StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length < 2) continue;

                var subgraph = Guid.NewGuid();

                var firstToken = new Node()
                {
                    ModifiedAndUse = false,
                    Modified = 0,
                    Operation = Operation.Write,
                    ParentId = parentNode.Id,
                    Text = Regex.Replace(tokens[0], @"{|}|;", String.Empty).Trim(),
                    Tooltip = Regex.Replace(tokens[0], @"{|}|;", String.Empty).Trim(),
                    Type = NodeType.ActionMethod,
                    Shape = NodeShape.Egg,
                    SubGraph = subgraph
                };

                var secondToken = new Node()
                {
                    ModifiedAndUse = false,
                    Modified = 0,
                    Operation = Operation.Read,
                    ParentId = parentNode.Id,
                    Text = Regex.Replace(tokens[1], @"{|}|;", String.Empty).Trim(),
                    Tooltip = Regex.Replace(tokens[1], @"{|}|;", String.Empty).Trim(),
                    Type = NodeType.ActionMethod,
                    Shape = NodeShape.Box,
                    SubGraph = subgraph
                };

                firstToken.Edges.Add(new Edge()
                {
                    Parent = firstToken,
                    Child = secondToken,
                    EdgeArrowType = EdgeArrowType.None,
                    EdgeStyle = EdgeStyle.Dotted
                });
                
                graph.Add(firstToken);
                graph.Add(secondToken);
                dataFlowGraph.Add(firstToken);
                dataFlowGraph.Add(secondToken);

                if (previousNode != null)
                {
                    graph.AddEdge(previousNode, firstToken);
                }

                previousNode = secondToken;
            }

            return graph;
        }

        private static void BFSHelper(Node parentNode, Node currentNode, Graph graph, Dictionary<Guid, Graph> graphs, ref Queue<Node> queue)
        {
            foreach (var edge in currentNode.Edges)
            {
                var childNode = edge.Child;

                if (graphs.ContainsKey(parentNode.Id))
                {
                    if (!graphs.ContainsKey(childNode.Id))
                    {
                        BFSHelper(parentNode, childNode, graph, graphs, ref queue);
                        continue;
                    }

                    var goNodes = MainNodes(graphs[childNode.Id]);
                    foreach (var endNode in EndNodes(graphs[parentNode.Id]))
                    {
                        foreach (var goNode in goNodes)
                        {
                            graph.AddEdge(endNode, goNode, edge.Color);
                        }
                    }
                }

                if (!queue.Any(x => x.Id == childNode.Id) && childNode.FillColor == Color.White)
                {
                    queue.Enqueue(childNode);
                }
            }
        }

        private static List<Node> MainNodes(Graph graph)
        {
            var notMainNodes = new List<Node>();
            graph.Nodes.ForEach((node) =>
            {
                foreach(var otherNode in graph.Nodes)
                {
                    foreach(var edge in otherNode.Edges)
                    {
                        if (edge.Child == node) notMainNodes.Add(node);
                    }
                }
            });

            return graph.Nodes.Except(notMainNodes).ToList();
        }

        private static List<Node> EndNodes(Graph graph)
        {
            return graph.Nodes.Where(x => !x.Edges.Any(y => y.Child.ParentId == x.ParentId)).ToList();
        }
        #endregion

        #region Helper Methods
        private static (List<String>, String) Pop(List<String> list)
        {
            if (list == null || !list.Any())
            {
                return (list, String.Empty);
            }

            var currentFirst = list[0];
            list.RemoveAt(0);
            return (list, currentFirst);
        }
        #endregion
    }
}
