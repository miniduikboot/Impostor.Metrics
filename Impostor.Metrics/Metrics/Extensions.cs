using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Impostor.Metrics.Metrics
{
    static partial class Extensions
    {
        public static IServiceCollection AddStatusCollectors(this IServiceCollection collection)
        {
            collection.AddSingleton<CpuStatus>();
            collection.AddSingleton<EventStatus>();
            collection.AddSingleton<GameStatus>();
            collection.AddSingleton<ThreadStatus>();
            return collection;
        }
    }
}
