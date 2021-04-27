using Impostor.Metrics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.MetricServer;

namespace Impostor.Metrics
{
    internal static class Extensions
    {
        public static IServiceCollection AddStatusCollectors(this IServiceCollection collection)
        {
            collection.AddSingleton<CpuStatus>();
            collection.AddSingleton<MemoryStatus>();
            collection.AddSingleton<EventStatus>();
            collection.AddSingleton<GameStatus>();
            collection.AddSingleton<ThreadStatus>();
            return collection;
        }
    }
}
