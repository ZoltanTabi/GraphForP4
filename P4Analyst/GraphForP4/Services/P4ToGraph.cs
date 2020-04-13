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
        public static Graph ControlFlowGraph(string input)
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
            var graph = new Graph();
            graph.Add(controlFlowGraph[0]);

            var queue = new Queue<Node>();
            foreach(var edge in controlFlowGraph[0].Edges)
            {
                edge.Child.FillColor = Color.Gray;
                queue.Enqueue(edge.Child);
            }

            while (queue.Any())
            {
                var node = queue.Dequeue();
                foreach (var edge in node.Edges)
                {
                    var childNode = edge.Child;

                    switch(childNode.Type)
                    {
                        case NodeType.If:
                            IfNode();
                            break;

                        case NodeType.Table:
                            TableNode();
                            break;

                        case NodeType.ActionMethod:
                            ActionMethodNode();
                            break;
                    }

                    if(childNode.FillColor == Color.White)
                    {
                        queue.Enqueue(childNode);
                    }
                }
                node.FillColor = Color.Black;
            }

            return graph;
        }

        private static void IfNode()
        {

        }

        private static void TableNode()
        {

        }

        private static void ActionMethodNode()
        {

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
