using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

        public static void Remove(this ISession session, Key key)
        {
            session.Remove(key.ToString("g"));
        }
    }
}
