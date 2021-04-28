using System.Diagnostics.Tracing;
using Impostor.Metrics.Gc.Collector;
using Microsoft.Extensions.DependencyInjection;

namespace Impostor.Metrics.Gc
{
    public static class Extensions
    {
        public static IServiceCollection AddGcListener(this IServiceCollection services)
        {
            const EventLevel level = EventLevel.Verbose;

            var listener = new GcEventListener(level);
            services.AddSingleton(listener);

            return services;
        }

        public static IServiceCollection AddGcSeries(this IServiceCollection services)
        {
            return services.AddSingleton<RegisterMetrics>();
        }

        public static IServiceCollection AddAggregator(this IServiceCollection services)
        {
            return services.AddSingleton<GcDataAggregation>();
        }
    }
}