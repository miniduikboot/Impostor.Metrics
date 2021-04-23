using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Impostor.Metrics.Metrics
{
    public class ThreadStatus
    {
        private readonly Process _proc;

        public int Total => _proc.Threads.Count;

        public int Pooled => ThreadPool.ThreadCount;

        public ThreadStatus()
        {
            this._proc = Process.GetCurrentProcess();
        }
    }
}
