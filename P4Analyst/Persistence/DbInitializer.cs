using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Persistence
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<P4Context>();

            context.Database.EnsureCreated();

            /*if (context.P4Files.Any())
            {
                return;
            }*/
        }
    }
}
