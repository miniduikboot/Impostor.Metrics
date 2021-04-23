using System;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Metrics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus.Client.DependencyInjection;

namespace Impostor.Metrics
{
    public class ImpostorStartup : IPluginStartup
    {
        public void ConfigureHost(IHostBuilder host) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMetricFactory();
            services.AddStatusCollectors();
            services.AddSingleton<PrometheusServer>();
        }
    }
}
