using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impostor.Metrics.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Impostor.Metrics.Metrics
{
    public class StatusManager
    {
        private readonly List<IMetricStatus> _exports;

        public StatusManager(StatusConfiguration config, IServiceProvider serviceProvider, ILogger<StatusManager> logger)
        {
            this._exports = new List<IMetricStatus>();
            if (config.EnableEventStatus)
            {
                _exports.Add((IMetricStatus)serviceProvider.GetRequiredService(typeof(EventStatus)));
            }
            if (config.EnableCpuStatus)
            {
                _exports.Add((IMetricStatus)serviceProvider.GetRequiredService(typeof(CpuStatus)));
            }
            if (config.EnableGameStatus)
            {
                _exports.Add((IMetricStatus)serviceProvider.GetRequiredService(typeof(GameStatus)));
            }
            if (config.EnableMemoryStatus)
            {
                _exports.Add((IMetricStatus)serviceProvider.GetRequiredService(typeof(MemoryStatus)));
            }
            if (config.EnableThreadStatus)
            {
                _exports.Add((IMetricStatus)serviceProvider.GetRequiredService(typeof(ThreadStatus)));
            }

            logger.LogInformation($"Impostor.Metrics: {_exports.Count} exports were created.");
        }

        public void Update()
        {
            foreach (var status in _exports) status.Update();
        }
    }
}
