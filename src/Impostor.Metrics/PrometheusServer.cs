using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Impostor.Api.Events.Managers;
using Impostor.Metrics.Config;
using Impostor.Metrics.Export;
using Impostor.Metrics.Metrics;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.Client.MetricServer;

namespace Impostor.Metrics
{
    public class PrometheusServer : IPrometheusServer
    {
        private readonly ILogger<PrometheusServer> _logger;

        public StatusManager StatusManager { get; }

        private readonly IMetricServer _server;

        private readonly Timer _clock;

        public PrometheusServer(ILogger<PrometheusServer> logger, StatusConfiguration config, ICollectorRegistry registry, StatusManager statusManager)
        {
            this._logger = logger;
            this.StatusManager = statusManager;

            this._server = new MetricServer(registry, new MetricServerOptions()
            {
                Port = config.Port,
                MapPath = config.ExportEndPoint,
                Host = config.Host
            });

            this._clock = new System.Timers.Timer(1000) { AutoReset = true };
            this._clock.Elapsed += Update;
        }

        public void Start()
        {
            _server.Start();
            _clock.Start();
            _logger.LogInformation("Impostor.Metrics: successfully started server.");
        }

        private void Update(object _, object __) => StatusManager.Update();

        public void Stop()
        {
            _server.Stop();
            _clock.Stop();
            _clock.Dispose();
        }
    }
}
