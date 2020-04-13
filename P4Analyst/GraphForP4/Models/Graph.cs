using GraphForP4.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public void AddEdge(Node parent, Node child)
        {
            parent.Edges.Add(new Edge
            {
                Parent = parent,
                Child = child
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
            var result = builder.ToString();

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

            builder.Append("],\"Type\":");
            builder.Append((int)node.Type);
            builder.Append(",\"FillColor\":");
            builder.Append(GetColorJson(node.FillColor));
            builder.Append(",\"FontColor\":");
            builder.Append(GetColorJson(node.FontColor));
            builder.Append(",\"Shape\":");
            builder.Append((int)node.Shape);
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
                string firstObject = string.Empty;
                string otherObjects = string.Empty;

                Color fillColor = new Color();
                Color fontColor = new Color();

                string nodeString2 = GetColor(nodeString, "FillColor", out fillColor);
                nodeString2 = GetColor(nodeString2, "FontColor", out fontColor);

                if (GetFirstJsonObject(nodeString2, "[]", out firstObject, out otherObjects))
                {
                    var node = DeserializeJson<Node>(nodeString2.Replace($",\"Edges\":{firstObject}", ""));
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
                    Child = Nodes.First(x => x.Id == Guid.Parse(attributes[1].Substring(9, attributes[1].Length - 10))),
                    Color = Color.FromName(attributes[2].Substring(9, attributes[2].Length - 11))
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

            color = Color.FromName(attributeValue.Substring(1, attributeValue.Length - 3));

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
