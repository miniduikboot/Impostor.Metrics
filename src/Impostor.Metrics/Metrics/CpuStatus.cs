using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Impostor.Metrics.Config;
using Microsoft.Extensions.Logging;
using Prometheus.Client;

namespace Impostor.Metrics.Metrics
{
    public class CpuStatus : IMetricStatus
    {
        private readonly Process _proc = Process.GetCurrentProcess();

        public float UsagePercent { get; private set; }

        private readonly IGauge<long> _cpuGauge;

        public CpuStatus(IMetricFactory metrics, ILogger<CpuStatus> logger)
        {
            Task.Factory.StartNew(CalculateUsage, TaskCreationOptions.LongRunning);

            this._cpuGauge = metrics.CreateGaugeInt64("cpu_percent", "CPU Usage percent (process only)");
            logger.LogInformation("Impostor.Metrics: enabled CPU status.");
        }

        private async Task CalculateUsage()
        {
            while (true)
            {
                var startTime = DateTime.Now;
                var usage = _proc.TotalProcessorTime;

                await Task.Delay(500);

                var endTime = DateTime.Now;

                var cpuTime = (float) ((_proc.TotalProcessorTime - usage).TotalMilliseconds);
                var duration = (Environment.ProcessorCount * ((endTime - startTime).TotalMilliseconds));

                this.UsagePercent = (float) Math.Round((cpuTime / duration)*100f, 2);
                this._proc.Refresh();
            }
        }

        public void Update()
        {
            this._cpuGauge.Set((long)this.UsagePercent);
        }
    }
}
