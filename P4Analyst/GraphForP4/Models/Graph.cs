using GraphForP4.Enums;
using GraphForP4.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GraphForP4.Models
{
    public class Graph
    {
        public List<Node> Nodes { get; set; } = new List<Node>();

        public Node this[int i]
        {
            get => Nodes[i];
            set => Nodes[i] = value;
        }

        public Node this[string text]
        {
            get => Nodes.FirstOrDefault(x => x.Text == text);
        }

        public void Add(Node node)
        {
            Nodes.Add(node);
        }

        public void AddEdge(Node parent, Node child, Color? color = null)
        {
            parent.Edges.Add(new Edge
            {
                Parent = parent,
                Child = child,
                Color = color.GetValueOrDefault(Color.Black)
            });
        }

        #region ToJson

        public string ToJson()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{\"Nodes\":[");

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                builder.Append(GetNodeJson(Nodes[i]));
            }

            builder.Append("]}");

            return builder.ToString();
        }

        private string GetNodeJson(Node node)
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
            if (node.Modified != null)
            {
                builder.Append(",\"Modified\":");
                builder.Append((int)node.Modified);
            }
            if (node.ModifiedAndUse != null)
            {
                builder.Append(",\"ModifiedAndUse\":");
                builder.Append(node.ModifiedAndUse.ToString().ToLower());
            }
            builder.Append("}");

            return builder.ToString();
        }

        private string GetEdgeJson(Edge edge)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{\"Parent\":\"");
            builder.Append(edge.Parent.Id);
            builder.Append("\",\"Child\":\"");
            builder.Append(edge.Child.Id);
            builder.Append("\",\"Color\":");
            builder.Append(GetColorJson(edge.Color));
            builder.Append(",\"EdgeArrowType\":");
            builder.Append((int)edge.EdgeArrowType);
            builder.Append(",\"EdgeStyle\":");
            builder.Append((int)edge.EdgeStyle);
            builder.Append("}");

            return builder.ToString();
        }

        private string GetColorJson(Color color)
        {
            return "\"" + color.Name + "\"";
        }

        #endregion

        #region FromJson

        public void FromJson(string jsonString)
        {
            List<string> nodeList = GetObjects(jsonString.Substring(1));
            Dictionary<Guid, List<string>> edges = new Dictionary<Guid, List<string>>();

            foreach (var nodeString in nodeList)
            {
                string processNodeString = GetColor(nodeString, "FillColor", out Color fillColor);
                processNodeString = GetColor(processNodeString, "FontColor", out Color fontColor);

                var firstObject = FileHelper.GetMethod(processNodeString, "\"Edges\":", '[', ']');

                if (firstObject.Length > 0)
                {
                    var node = DeserializeJson<Node>(processNodeString.Replace($",\"Edges\":{firstObject}", string.Empty));
                    node.FillColor = fillColor;
                    node.FontColor = fontColor;
                    Add(node);

                    edges.Add(node.Id, GetObjects(firstObject));
                }
            }

            foreach (var node in Nodes)
            {
                node.Edges = GetEdges(edges.First(x => x.Key == node.Id).Value, node);
                if(node.Type == NodeType.ActionMethod)
                {
                    node.Text = Regex.Replace(node.Text, @" *; *", ";" + Environment.NewLine);
                }
            }
        }

        private List<string> GetObjects(string jsonString)
        {
            List<string> result = new List<string>();

            string firstObject = string.Empty;
            string otherObjects = string.Empty;

            while (GetFirstJsonObject(jsonString, "{}", out firstObject, out otherObjects))
            {
                result.Add(firstObject);
                jsonString = otherObjects;
            }

            return result;
        }

        private bool GetFirstJsonObject(string jsonString, string firstCharPair, out string firstObject, out string otherObjects)
        {
            bool result = false;
            firstObject = string.Empty;
            otherObjects = string.Empty;

            char firstChar = firstCharPair[0];
            char lastChar = firstCharPair[1];
            int firstPos = -1;
            int lastPos = -1;
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
                    lastPos = i;

                    firstObject = jsonString.Substring(firstPos, lastPos - firstPos + 1);
                    otherObjects = jsonString.Substring(lastPos + 1);
                    result = true;

                    break;
                }
            }

            return result;
        }

        private List<Edge> GetEdges(List<string> edges, Node node)
        {
            List<Edge> result = new List<Edge>();

            foreach (var edgeString in edges)
            {
                var attributes = edgeString.Split(",");

                result.Add(new Edge
                {
                    Parent = node,
                    Child = Nodes.First(x => x.Id == Guid.Parse(attributes[1][9..^1])),
                    Color = Color.FromName(attributes[2].Substring(9, attributes[2].Length - 10)),
                    EdgeArrowType = (EdgeArrowType)Convert.ToInt32(attributes[3].Split(':')[1]),
                    EdgeStyle = (EdgeStyle)Convert.ToInt32(attributes[4].Split(':', '}')[1])
                });
            }

            return result;
        }

        private string GetColor(string jsonString, string attributeName, out Color color)
        {
            string result = jsonString;
            color = new Color();

            int attributePos = jsonString.IndexOf(attributeName);
            string lastString = jsonString.Substring(attributePos);
            int lastPos = lastString.IndexOf(",");
            string attributePair = result.Substring(attributePos - 1, lastPos + 2);
            string attributeValue = attributePair.Split(":")[1];

            color = Color.FromName(attributeValue[1..^2]);

            result = result.Replace(attributePair, "");

            return result;
        }

        private T DeserializeJson<T>(string jsonString)
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        #endregion 

    }
}
