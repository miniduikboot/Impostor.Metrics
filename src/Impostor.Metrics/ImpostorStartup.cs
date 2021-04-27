using System;
using System.IO;
using Impostor.Api.Plugins;
using Impostor.Metrics.Config;
using Impostor.Metrics.Export;
using Impostor.Metrics.Metrics;
using Microsoft.Extensions.Configuration;
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
            Configure(services);

            services.AddMetricFactory();
            services.AddStatusCollectors();
            services.AddSingleton<IPrometheusServer, PrometheusServer>();
        }

        private static void Configure(IServiceCollection services)
        {
            var configuration = CreateConfiguration();

            var statusConfig = configuration
                .GetSection(StatusConfiguration.Section)
                .Get<StatusConfiguration>() ?? new StatusConfiguration();

            statusConfig.ValidateConfig();

            services.AddSingleton(statusConfig);
        }

        private static IConfiguration CreateConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("metrics.json", true);
            configurationBuilder.AddEnvironmentVariables(prefix: "IMPOSTOR.METRICS_");

            return configurationBuilder.Build();
        }
    }
}
