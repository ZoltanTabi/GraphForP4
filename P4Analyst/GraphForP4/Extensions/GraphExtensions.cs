using GraphForP4.Enums;
using GraphForP4.Helpers;
using GraphForP4.Models;
using GraphForP4.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GraphForP4.Extensions
{
    public static class GraphExtensions
    {
        #region Common

        public static List<Node> MainNodes(this Graph graph)
        {
            return graph.Nodes.MainNodes();
        }

        public static List<Node> MainNodes(this List<Node> nodes)
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

        public static List<Node> EndNodes(this Graph graph)
        {
            return graph.Nodes.Where(x => !x.Edges.Any(y => y.Child.ParentId == x.ParentId)).ToList();
        }

        #endregion

        #region Serialize

        public static IEnumerable<AngularNode> Serialize(this Graph graph)
        {
            var angularGraph = new List<AngularNode>();

            var level = 0;
            var currentNodes = graph.MainNodes();

            while (currentNodes.Any())
            {
                (currentNodes, level) = GenerateLevel(angularGraph, currentNodes, level);
            }

            return angularGraph;
        }

        private static (List<Node>, int) GenerateLevel(List<AngularNode> angularGraph, List<Node> currentNodes, int level)
        {
            var childNodes = new List<Node>();
            currentNodes.ForEach(node =>
            {
                var item = new AngularNode
                {
                    Text = node.Text,
                    Id = node.Id.ToString().Replace("-", String.Empty),
                    Number = level,
                    FillColor = $"#{node.FillColor.R:X2}{node.FillColor.G:X2}{node.FillColor.B:X2}",
                    FontColor = $"#{node.FontColor.R:X2}{node.FontColor.G:X2}{node.FontColor.B:X2}",
                    Shape = (int)node.Shape,
                    Tooltip = node.Tooltip,
                    ParentId = node.ParentId != null ? node.ParentId.ToString().Replace("-", String.Empty) : String.Empty,
                    SubGraph = node.SubGraph != null ? "cluster" + node.SubGraph.ToString().Replace("-", String.Empty) : String.Empty,
                    Edges = new List<AngularEdge>()
                };

                foreach (var edge in node.Edges)
                {
                    item.Edges.Add(new AngularEdge()
                    {
                        Parent = edge.Parent.Id.ToString().Replace("-", String.Empty),
                        Child = edge.Child.Id.ToString().Replace("-", String.Empty),
                        Color = $"#{edge.Color.R:X2}{edge.Color.G:X2}{edge.Color.B:X2}",
                        ArrowType = (int)edge.ArrowType,
                        Style = (int)edge.Style
                    });
                    childNodes.Add(edge.Child);
                }

                angularGraph.Add(item);
            });

            CheckExisting(childNodes, angularGraph);
            return (childNodes.Distinct().ToList(), ++level);
        }

        private static void CheckExisting(List<Node> childNodes, List<AngularNode> angularGraph)
        {
            foreach (var angularNode in angularGraph)
            {
                for (var i = childNodes.Count - 1; i >= 0; --i)
                {
                    if (childNodes[i].Id.ToString().Replace("-", String.Empty) == angularNode.Id) childNodes.Remove(childNodes[i]);
                }
            }
        }

        #endregion

        #region ToJson

        public static string ToJson(this Graph graph)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{\"Nodes\":[");

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                builder.Append(GetNodeJson(graph.Nodes[i]));
            }

            return builder.Append("]}").ToString();
        }

        private static string GetNodeJson(Node node)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{\"Text\":\"");
            builder.Append(node.Text);
            builder.Append("\",\"Id\":\"");
            builder.Append(node.Id);
            builder.Append("\",\"Edges\":[");

            for (int i = 0; i < node.Edges.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                builder.Append(GetEdgeJson(node.Edges[i]));
            }

            builder.Append("],\"Tooltip\":\"");
            builder.Append(node.Tooltip);
            builder.Append("\",\"Type\":");
            builder.Append((int)node.Type);
            builder.Append(",\"FillColor\":");
            builder.Append(GetColorJson(node.FillColor));
            builder.Append(",\"FontColor\":");
            builder.Append(GetColorJson(node.FontColor));
            builder.Append(",\"Shape\":");
            builder.Append((int)node.Shape);
            builder.Append(",\"Use\":");
            builder.Append(node.Use);
            if (node.ParentId != null)
            {
                builder.Append(",\"ParentId\":\"");
                builder.Append(node.ParentId);
                builder.Append("\"");
            }
            if (node.SubGraph != null)
            {
                builder.Append(",\"SubGraph\":\"");
                builder.Append(node.SubGraph);
                builder.Append("\"");
            }
            if (node.Operation != null)
            {
                builder.Append(",\"Operation\":");
                builder.Append((int)node.Operation);
            }

            return builder.Append("}").ToString();
        }

        private static string GetEdgeJson(Edge edge)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{\"Parent\":\"");
            builder.Append(edge.Parent.Id);
            builder.Append("\",\"Child\":\"");
            builder.Append(edge.Child.Id);
            builder.Append("\",\"Color\":");
            builder.Append(GetColorJson(edge.Color));
            builder.Append(",\"EdgeArrowType\":");
            builder.Append((int)edge.ArrowType);
            builder.Append(",\"EdgeStyle\":");
            builder.Append((int)edge.Style);
            builder.Append("}");

            return builder.ToString();
        }

        private static string GetColorJson(Color color)
        {
            return "\"" + color.Name + "\"";
        }

        #endregion

        #region FromJson

        public static void FromJson(this Graph graph, string jsonString)
        {
            List<string> nodeList = GetObjects(jsonString.Substring(1));
            Dictionary<Guid, List<string>> edges = new Dictionary<Guid, List<string>>();

            foreach (var nodeString in nodeList)
            {
                string processNodeString = GetColor(nodeString, "FillColor", out Color fillColor);
                processNodeString = GetColor(processNodeString, "FontColor", out Color fontColor);

                var firstObject = FileHelper.GetMethod(processNodeString, "Edges", '[', ']', true);

                if (firstObject.Length > 0)
                {
                    var node = DeserializeJson<Node>(processNodeString.Replace($",\"Edges\":{firstObject}", string.Empty));
                    node.FillColor = fillColor;
                    node.FontColor = fontColor;
                    graph.Add(node);

                    edges.Add(node.Id, GetObjects(firstObject));
                }
            }

            foreach (var node in graph.Nodes)
            {
                node.Edges = GetEdges(graph, edges.First(x => x.Key == node.Id).Value, node);
                if (node.Type == NodeType.ActionMethod)
                {
                    node.Text = Regex.Replace(node.Text, @" *; *", ";" + Environment.NewLine);
                    if (node.Text.Contains('{'))
                    {
                        node.Text = node.Text.Substring(node.Text.IndexOf('{'), 1);
                    }
                    if (node.Text.Contains('}'))
                    {
                        node.Text = node.Text.Substring(node.Text.LastIndexOf('}'), 1);
                    }
                }
            }
        }

        private static List<string> GetObjects(string jsonString)
        {
            List<string> result = new List<string>();

            while (GetFirstJsonObject(jsonString, "{}", out string firstObject, out string otherObjects))
            {
                result.Add(firstObject);
                jsonString = otherObjects;
            }

            return result;
        }

        private static bool GetFirstJsonObject(string jsonString, string firstCharPair, out string firstObject, out string otherObjects)
        {
            bool result = false;
            firstObject = string.Empty;
            otherObjects = string.Empty;

            char firstChar = firstCharPair[0];
            char lastChar = firstCharPair[1];
            int firstPos = -1;
            int firstCharNumber = 0;
            int lastCharNumber = 0;

            for (int i = 0; i < jsonString.Length; i++)
            {
                if (jsonString[i] == firstChar)
                {
                    if (firstPos == -1)
                    {
                        firstPos = i;
                    }
                    firstCharNumber++;
                }
                if (jsonString[i] == lastChar)
                {
                    lastCharNumber++;
                }

                if (firstPos != -1 && firstCharNumber == lastCharNumber)
                {
                    int lastPos = i;

                    firstObject = jsonString.Substring(firstPos, lastPos - firstPos + 1);
                    otherObjects = jsonString.Substring(lastPos + 1);
                    result = true;

                    break;
                }
            }

            return result;
        }

        private static List<Edge> GetEdges(Graph graph, List<string> edges, Node node)
        {
            List<Edge> result = new List<Edge>();

            foreach (var edgeString in edges)
            {
                var attributes = edgeString.Split(",");

                result.Add(new Edge
                {
                    Parent = node,
                    Child = graph.Nodes.First(x => x.Id == Guid.Parse(attributes[1][9..^1])),
                    Color = Color.FromName(attributes[2][9..^1]),
                    ArrowType = (EdgeArrowType)Convert.ToInt32(attributes[3].Split(':')[1]),
                    Style = (EdgeStyle)Convert.ToInt32(attributes[4].Split(':', '}')[1])
                });
            }

            return result;
        }

        private static string GetColor(string jsonString, string attributeName, out Color color)
        {
            string result = jsonString;

            int attributePos = jsonString.IndexOf(attributeName);
            string lastString = jsonString.Substring(attributePos);
            int lastPos = lastString.IndexOf(",");
            string attributePair = jsonString.Substring(attributePos - 1, lastPos + 2);
            string attributeValue = attributePair.Split(":")[1];

            color = Color.FromName(attributeValue[1..^2]);
            
            return jsonString.Replace(attributePair, "");
        }

        private static T DeserializeJson<T>(string jsonString)
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        #endregion 


    }
}
