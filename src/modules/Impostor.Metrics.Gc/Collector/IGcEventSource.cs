using System;
using Impostor.Metrics.Gc.Collector.Notifications;

namespace Impostor.Metrics.Gc.Collector
{
    public interface IGcEventSource
    {
        public event Action<GcHeapNotification> HeapStats;
        public event Action<GcAllocationNotification> AllocationTick;
        public event Action<GcCollectionStartNotification> CollectionStart;
        public event Action<GcCollectionEndNotification> CollectionComplete;
        public event Action<GcPauseCompleteNotification> PauseComplete;
    }
}