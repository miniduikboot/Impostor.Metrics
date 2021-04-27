using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Impostor.Metrics.Config;
using Microsoft.Extensions.Logging;
using Prometheus.Client;

namespace Impostor.Metrics.Metrics
{
    public class ThreadStatus : IMetricStatus
    {
        private readonly Process _proc;

        public int Total => _proc.Threads.Count;

        public int Pooled => ThreadPool.ThreadCount;

        private readonly IGauge<long> _totalThreads;

        private readonly IGauge<long> _threadPoolThreads;


        public ThreadStatus(IMetricFactory metrics, ILogger<ThreadStatus> logger)
        {
            this._proc = Process.GetCurrentProcess();

            this._totalThreads = metrics.CreateGaugeInt64("total_threads", "Total process threads.");
            this._threadPoolThreads = metrics.CreateGaugeInt64("pool_threads", "Total Thread Pool threads.");
            logger.LogInformation("Impostor.Metrics: enabled thread status.");
        }

        public void Update()
        {
            this._totalThreads.Set(this.Total);
            this._threadPoolThreads.Set(this.Pooled);
        }
    }
}
