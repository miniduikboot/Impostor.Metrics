using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impostor.Metrics.Config;
using Microsoft.Extensions.Logging;
using Prometheus.Client;

namespace Impostor.Metrics.Metrics
{
    public class MemoryStatus : IMetricStatus
    {
        private readonly Process _proc;

        public long PeakPagedMemory
        {
            get
            {
                _proc.Refresh();
                return _proc.PeakPagedMemorySize64;
            }
        }

        public long PeakWorkingSet
        {
            get
            {
                _proc.Refresh();
                return _proc.PeakWorkingSet64;
            }
        }

        public long PeakVirtualMemory
        {
            get
            {
                _proc.Refresh();
                return _proc.PeakVirtualMemorySize64;
            }
        }

        public long WorkingSet
        {
            get
            {
                _proc.Refresh();
                return _proc.WorkingSet64;
            }
        }

        public long PagedMemorySize
        {
            get
            {
                _proc.Refresh();
                return _proc.PagedMemorySize64;
            }
        }

        public long PagedSystemMemorySize
        {
            get
            {
                _proc.Refresh();
                return _proc.PagedSystemMemorySize64;
            }
        }

        public long PrivateBytes
        {
            get
            {
                _proc.Refresh();
                return _proc.PrivateMemorySize64;
            }
        }

        private readonly IGauge<long> _privateMemory;

        private readonly IGauge<long> _peakPagedMemory;

        private readonly IGauge<long> _peakWorkingSet;

        private readonly IGauge<long> _peakVirtualMemory;

        private readonly IGauge<long> _workingSet;

        private readonly IGauge<long> _pagedMemory;

        private readonly IGauge<long> _pagedSystemMemory;

        public MemoryStatus(IMetricFactory metrics, ILogger<MemoryStatus> logger)
        {
            this._proc = Process.GetCurrentProcess();

            this._privateMemory = metrics.CreateGaugeInt64("memory_bytes", "The Memory Usage");
            this._peakPagedMemory = metrics.CreateGaugeInt64("peak_paged_memory_bytes", "N/A");
            this._peakWorkingSet = metrics.CreateGaugeInt64("peak_working_set_bytes", "N/A");
            this._peakVirtualMemory = metrics.CreateGaugeInt64("peak_virtual_memory_bytes", "N/A");
            this._workingSet = metrics.CreateGaugeInt64("working_set_bytes", "N/A");
            this._pagedMemory = metrics.CreateGaugeInt64("paged_memory_bytes", "N/A");
            this._pagedSystemMemory = metrics.CreateGaugeInt64("paged_system_memory_bytes", "N/A");
            logger.LogInformation("Impostor.Metrics: enabled memory status.");
        }

        public void Update()
        {
            this._peakPagedMemory.Set(this.PeakPagedMemory);
            this._peakWorkingSet.Set(this.PeakWorkingSet);
            this._peakVirtualMemory.Set(this.PeakVirtualMemory);
            this._workingSet.Set(this.WorkingSet);
            this._pagedMemory.Set(this.PagedMemorySize);
            this._pagedSystemMemory.Set(this.PagedSystemMemorySize);
            this._privateMemory.Set(this.PrivateBytes);
        }
    }
}
