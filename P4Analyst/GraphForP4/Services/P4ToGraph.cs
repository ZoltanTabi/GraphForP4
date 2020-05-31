using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using GraphForP4.Enums;
using GraphForP4.Models;
using GraphForP4.Helpers;
using GraphForP4.Extensions;

namespace GraphForP4.Services
{
    public static class P4ToGraph
    {
        private const string APPLY = "apply";
        private const string IF = "if";
        private const string ELSE = "else";
        private const string ACTIONS = "actions";

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

            if (Regex.IsMatch(string.Join(" ", applyMethod), @"else +if", RegexOptions.IgnoreCase)) throw new ApplicationException("Nem megengedett nyelvi elem! (else if)");

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
                for(var j = graph[i].Edges.Count -1; j >= 0; --j)
                {
                    if(graph[i].Edges[j].Child.Type == NodeType.Skip)
                    {
                        for(var k = graph[i].Edges[j].Child.Edges.Count - 1; k >= 0; --k)
                        {
                            graph.AddEdge(graph[i].Edges[j].Parent, graph[i].Edges[j].Child.Edges[k].Child, Color.Red);
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
                        currentNodes = TableMethod(graph, currentNodes, Regex.Replace(current, @"\. *" + APPLY + @" *\( *\);", String.Empty).Trim(), ingressMethod, ref edgeColor);
                    }
                    else if (current.Contains('('))
                    {
                        currentNodes = ActionMethod(graph, currentNodes, Regex.Replace(current, @" *\(.*\);", String.Empty).Trim(), ref edgeColor);
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
                                   Regex.Replace(action, @"( |;)", String.Empty).Trim(), ref edgeColor));
                }
            }

            return currentNodes;
        }

        private static List<Node> ActionMethod(Graph graph, List<Node> currentNodes, String actionName, ref Color? edgeColor)
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

            return new List<Node> { actionNode };
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

                    case NodeType.Action:
                        var actionGraph = ActionNode(node, graph, input);
                        if (actionGraph != null && actionGraph.Nodes.Any())
                        {
                            graphs.Add(node.Id, actionGraph);
                        }
                        break;

                    case NodeType.ActionMethod:
                        graphs.Add(node.Id, ActionMethodNode(node, graph));
                        break;
                }
            }

            graphs.First().Value.MainNodes().ForEach(fromStartNode =>
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

                BFSHelper(node, node, graph, graphs, queue);

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
                    graphs[controlFlowGraphNode.Id].EndNodes().ForEach(node =>
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

            Guid? subGraph = null;
            if (split.Count > 1)
            {
                subGraph = Guid.NewGuid();
            }

            foreach (var text in split)
            {
                var node = new Node()
                {
                    Operation = Operation.Read,
                    ParentId = parentNode.Id,
                    Text = text.Trim(),
                    Tooltip = "if",
                    Type = NodeType.If,
                    Shape = NodeShape.Box,
                    SubGraph = subGraph
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
                        ArrowType = EdgeArrowType.None,
                        Style = EdgeStyle.Dotted
                    });
                }
            }

            return graph;
        }

        private static Graph TableNode(Node parentNode, Graph dataFlowGraph, string input)
        {
            var graph = new Graph();

            var ingressMethod = FileHelper.GetMethod(input, FileHelper.GetIngressControlName(input));
            var tableMethod = FileHelper.GetMethod(ingressMethod, "table " + parentNode.Text);

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

        private static Graph ActionNode(Node parentNode, Graph dataFlowGraph, string input)
        {
            var ingressMethod = FileHelper.GetMethod(input, FileHelper.GetIngressControlName(input));
            var actionMethod = FileHelper.GetMethod(ingressMethod, "action " + parentNode.Text).Trim();
            actionMethod = Regex.Replace(actionMethod, @" +", " ");

            if (String.IsNullOrWhiteSpace(actionMethod))
            {
                return null;
            }

            var actionMethodNode = new Node
            {
                Text = actionMethod,
                Type = NodeType.ActionMethod,
                Id = parentNode.Id
            };

            return ActionMethodNode(actionMethodNode, dataFlowGraph);
        }

        private static Graph ActionMethodNode(Node parentNode, Graph dataFlowGraph)
        {
            if (Regex.IsMatch(parentNode.Text, @" if *\(", RegexOptions.IgnoreCase)) throw new ApplicationException("Nem megengedett nyelvi elem! (Akción belüli elágazás)");

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
                    ArrowType = EdgeArrowType.None,
                    Style = EdgeStyle.Dotted
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

        private static void BFSHelper(Node parentNode, Node currentNode, Graph graph, Dictionary<Guid, Graph> graphs, Queue<Node> queue)
        {
            foreach (var edge in currentNode.Edges)
            {
                var childNode = edge.Child;

                if (graphs.ContainsKey(parentNode.Id))
                {
                    if (!graphs.ContainsKey(childNode.Id))
                    {
                        BFSHelper(parentNode, childNode, graph, graphs, queue);
                        continue;
                    }

                    var goNodes = graphs[childNode.Id].MainNodes();
                    foreach (var endNode in graphs[parentNode.Id].EndNodes())
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

        #endregion

        #region Helper Method
        private static (List<T>, T) Pop<T>(List<T> list)
        {
            if (list == null || !list.Any())
            {
                return (list, default(T));
            }

            var currentFirst = list[0];
            list.RemoveAt(0);
            return (list, currentFirst);
        }
        #endregion
    }
}
