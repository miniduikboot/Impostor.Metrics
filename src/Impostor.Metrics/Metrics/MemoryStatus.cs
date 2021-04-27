using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impostor.Metrics.Config;

namespace Impostor.Metrics.Metrics
{
    public class MemoryStatus
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

        public MemoryStatus(StatusConfiguration configuration)
        {
            if(!configuration.EnableMemoryStatus) return;
            this._proc = Process.GetCurrentProcess();
        }
    }
}
