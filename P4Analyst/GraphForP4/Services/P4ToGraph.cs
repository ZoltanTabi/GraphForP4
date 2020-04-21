using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphForP4.Enums;
using GraphForP4.Models;

namespace GraphForP4.Services
{
    public static class P4ToGraph
    {
        const string APPLY = "apply";
        const string IF = "if";
        const string ELSE = "else";
        const string ACTIONS = "actions";
        private static readonly String[] CHARACTERS = { " ", "(", "{", "=" };

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

            input = InputClean(input);
            var matchString = Regex.Match(input, "V1Switch(.*)main").Value;
            var ingressControlName = Regex.Split(matchString, @"\(([^\(]*)\)([^,]*),")
                                          .Where(x => !string.IsNullOrWhiteSpace(x)).ToList()[2].Trim();
            
            var ingressMethod = GetMethod(input, ingressControlName);
            var applyMethod = SplitAndClean(GetMethod(ingressMethod, APPLY));

            currentNodes = Search(graph, currentNodes, applyMethod, ingressMethod);

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
                            graph.AddEdge(graph.Nodes[i].Edges[j].Parent, graph.Nodes[i].Edges[j].Child.Edges[k].Child);
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

        private static List<Node> Search(Graph graph, List<Node> currentNodes, List<String> method, String ingressMethod)
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
                                       ingressMethod);
                    }
                    else if (current.Contains('('))
                    {
                        currentNodes = ActionMethod(graph, currentNodes,
                                       Regex.Replace(current, @"\. *" + APPLY + @" *\( *\);", String.Empty).Trim(),
                                       ingressMethod);
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
                            graph.AddEdge(node, actionMethodNode);
                        }

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

                    currentNodes = IfMethod(graph, currentNodes, ifMethod + elseMethod, ingressMethod);
                }
                (method, current) = Pop(method);
            }

            return currentNodes;
        }

        private static List<Node> IfMethod(Graph graph, List<Node> currentNodes, String ifMethod, String ingressMethod)
        {
            var ifCondition = GetMethod(ifMethod, IF, '(', ')');
            ifCondition = ifCondition.Insert(0, $"{IF} ");
            var ifNode = new Node
            {
                Text = ifCondition,
                Type = NodeType.If,
                Tooltip = ifCondition
            };

            graph.Add(ifNode);

            foreach(var node in currentNodes)
            {
                graph.AddEdge(node, ifNode);
            }

            var ifTrueMethod = GetMethod(ifMethod, ifCondition);
            ifMethod = ifMethod.Replace(IF, String.Empty).Replace(ifCondition, String.Empty).Replace(ifTrueMethod, String.Empty);
            currentNodes = Search(graph, new List<Node> { ifNode }, SplitAndClean(ifTrueMethod), ingressMethod);

            if(ifMethod.Contains(ELSE))
            {
                var elseMethod = SplitAndClean(GetMethod(ifMethod, ELSE));
                currentNodes = currentNodes.Concat(Search(graph, new List<Node> { ifNode }, elseMethod, ingressMethod)).ToList();
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

        private static List<Node> TableMethod(Graph graph, List<Node> currentNodes, String tableName, String ingressMethod)
        {
            var tableNode = new Node
            {
                Text = tableName,
                Type = NodeType.Table,
                Tooltip = tableName
            };

            graph.Add(tableNode);
            
            foreach(var node in currentNodes)
            {
                graph.AddEdge(node, tableNode);
            }

            currentNodes = new List<Node>();

            var tableMethod = GetMethod(ingressMethod, "table " + tableName);
            var actions = SplitAndClean(GetMethod(tableMethod, ACTIONS));
            foreach(var action in actions)
            {
                if(action.Contains(";"))
                {
                    currentNodes = currentNodes
                                   .Concat(ActionMethod(graph, new List<Node> { tableNode },
                                   Regex.Replace(action, @"( |;)", String.Empty).Trim(), ingressMethod)).ToList();
                }
            }

            return currentNodes;
        }

        private static List<Node> ActionMethod(Graph graph, List<Node> currentNodes, String actionName, String ingressMethod)
        {
            var actionNode = new Node
            {
                Text = actionName,
                Type = NodeType.Action,
                Tooltip = actionName
            };

            graph.Add(actionNode);
            foreach(var node in currentNodes)
            {
                graph.AddEdge(node, actionNode);
            }

            var actionMethod = GetMethod(ingressMethod, "action " + actionName).Trim();
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
            var graph = new Graph();
            graph.Add(new Node()
            {
                Text = "Start",
                FillColor = Color.Green,
                Tooltip = "Start",
                Shape = NodeShape.Diamond
            });

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

            var queue = new Queue<Node>();
            foreach(var edge in controlFlowGraph[0].Edges)
            {
                edge.Child.FillColor = Color.Gray;
                queue.Enqueue(edge.Child);
            }

            while (queue.Any())
            {
                var node = queue.Dequeue();
                node.FillColor = Color.Gray;

                BFSHelper(node, node, graph, graphs, ref queue);

                node.FillColor = Color.Black;
            }

            return graph;
        }

        private static Graph IfNode(Node parentNode, Graph dataFlowGraph)
        {
            var graph = new Graph();

            var condition = GetMethod(parentNode.Text, IF, '(', ')');
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
                    Tooltip = text.Trim(),
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

            var tableMethod = GetMethod(input, "table " + parentNode.Text);

            if (!tableMethod.Contains("key")) return graph;

            var keys = GetMethod(tableMethod, "key");

            if (keys.Trim().Length == 0) return graph;

            keys = keys.Remove(keys.Length - 1, 1).Remove(0, 1).Trim();

            Node previousNode = null;

            foreach (var dir in keys.Split(";", StringSplitOptions.RemoveEmptyEntries))
            {
                var key = dir.Split(":")[0];

                if(key.Trim().Length > 0)
                {
                    var node = new Node()
                    {
                        ModifiedAndUse = false,
                        Modified = 0,
                        Operation = Operation.Read,
                        ParentId = parentNode.Id,
                        Text = key.Trim(),
                        Tooltip = "Kulcs",
                        Type = NodeType.Key
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

            List<Node> previousNodes = null;

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

                    if (previousNodes != null)
                    {
                        foreach (var previousNode in previousNodes)
                        {
                            graph.AddEdge(previousNode, node);
                        }
                    }

                    previousNodes = new List<Node>
                    {
                        node
                    };

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
                    Shape = NodeShape.Circle,
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

                if (previousNodes != null)
                {
                    foreach (var node in previousNodes)
                    {
                        graph.AddEdge(node, firstToken);
                        graph.AddEdge(node, secondToken);
                    }
                }

                previousNodes = new List<Node>
                {
                    firstToken,
                    secondToken
                };
            }

            return graph;
        }

        private static void BFSHelper(Node parentNode, Node currentNode, Graph graph, Dictionary<Guid, Graph> graphs, ref Queue<Node> queue)
        {
            foreach (var edge in currentNode.Edges)
            {
                var childNode = edge.Child;

                if (!graphs.ContainsKey(childNode.Id))
                {
                    BFSHelper(parentNode, childNode, graph, graphs, ref queue);
                    continue;
                }

                var goNodes = MainNode(graphs[childNode.Id]);
                foreach (var endNode in EndNodes(graphs[parentNode.Id]))
                {
                    foreach(var goNode in goNodes)
                    {
                        graph.AddEdge(endNode, goNode);
                    }
                }

                if (!queue.Any(x => x.Id == childNode.Id) && childNode.FillColor == Color.White)
                {
                    queue.Enqueue(childNode);
                }
            }
        }

        private static List<Node> MainNode(Graph graph)
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
            return graph.Nodes.Where(x => !x.Edges.Any()).ToList();
        }
        #endregion

        #region Helper Methods
        private static string InputClean(string input)
        {
            input = Regex.Replace(input, @"(<[^0-9]*>)|(//(.*?)\r?\n)|(/\*(.*)\*/)|([\n\r])", " ");
            input = Regex.Unescape(input);

            return input;
        }

        private static string GetMethod(string input, string firstEqual, char startChar = '{', char endChar = '}')
        {
            var result = String.Empty;
            var count = 0;
            var findStartChar = false;
            var index = -1;
            
            for(var i = 0; i < CHARACTERS.Length && index == -1; ++i)
            {
                index = input.IndexOf(firstEqual + CHARACTERS[i]);
            }

            for(var i = index; i > -1 && (count != 0 || !findStartChar); ++i)
            {
                if(input[i] == startChar)
                {
                    findStartChar = true;
                    ++count;
                }
                else if(input[i] == endChar)
                {
                    --count;
                }
                if(findStartChar)
                {
                    result += input[i];
                }
            }

            return result;
        }

        private static List<String> SplitAndClean(String input)
        {
            var result = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            result.RemoveAt(0);
            result.RemoveAt(result.Count - 1);

            return result;
        }

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
