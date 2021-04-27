using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus.Client;
using Prometheus.Client.MetricServer;

namespace Impostor.Metrics.Export
{
    public interface IPrometheusServer
    {
        IMetricFactory Metrics { get; }

        void Start();

        void Stop();
    }
}
