using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Impostor.Metrics.Config;

namespace Impostor.Metrics.Metrics
{
    public class CpuStatus
    {
        private readonly Process _proc = Process.GetCurrentProcess();

        public float UsagePercent { get; private set; }

        public CpuStatus(StatusConfiguration configuration)
        {
            if (!configuration.EnableCpuStatus) return;
            Task.Factory.StartNew(Update, TaskCreationOptions.LongRunning);
        }

        private async Task Update()
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
    }
}
