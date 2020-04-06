using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GraphForP4.Models;

namespace AngularApp.Extensions
{
    public static class SessionExtension
    {
        public static void Set<T>(this ISession session, Key key, T value)
        {
            session.SetString(key.ToString("g"), JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, Key key)
        {
            var value = session.GetString(key.ToString("g"));
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public static void SetGraph(this ISession session, Key key, Graph graph)
        {
            session.SetString(key.ToString("g"), graph.ToJson());
        }

        public static Graph GetGraph(this ISession session, Key key)
        {
            var value = session.GetString(key.ToString("g"));
            var graph = new Graph();
            if (value != null)
            {
                graph.FromJson(value);
            }
            return graph;
        }

        public static void Remove(this ISession session, Key key)
        {
            session.Remove(key.ToString("g"));
        }
    }
}
