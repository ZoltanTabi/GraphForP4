using Microsoft.AspNetCore.Http;
using System.Text.Json;
using GraphForP4.Models;
using GraphForP4.Extensions;

namespace AngularApp.Extensions
{
    public static class SessionExtension
    {
        public static void Set<T>(this ISession session, Key key, T value)
        {
            session.Set(key.ToString("g"), System.Text.Encoding.ASCII.GetBytes(JsonSerializer.Serialize(value)));
        }

        public static T Get<T>(this ISession session, Key key)
        {
            var exist = session.TryGetValue(key.ToString("g"), out byte[] value);
            return exist ?  JsonSerializer.Deserialize<T>(System.Text.Encoding.ASCII.GetString(value)) : default;
        }

        public static void SetGraph(this ISession session, Key key, Graph graph)
        {
            session.Set(key.ToString("g"), System.Text.Encoding.ASCII.GetBytes(graph.ToJson()));
        }

        public static Graph GetGraph(this ISession session, Key key)
        {
            var exist = session.TryGetValue(key.ToString("g"), out byte[] value);
            var graph = new Graph();
            if (exist)
            {
                graph.FromJson(System.Text.Encoding.ASCII.GetString(value));
            }
            return graph;
        }

        public static void Remove(this ISession session, Key key)
        {
            session.Remove(key.ToString("g"));
        }
    }
}
