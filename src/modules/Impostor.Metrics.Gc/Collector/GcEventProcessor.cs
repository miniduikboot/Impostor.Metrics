using System;
using System.Diagnostics.Tracing;
using Impostor.Metrics.Gc.Collector.Duration;
using Impostor.Metrics.Gc.Collector.Notifications;

namespace Impostor.Metrics.Gc.Collector
{
    public class GcEventProcessor : IGcEventSource
    {
        public void ProcessEvent(EventWrittenEventArgs e)
        {
            // flags representing the "Garbage Collection" + "Preparation for garbage collection" pause reasons
            const uint suspendGcReasons = 0x1 | 0x6;

            switch (e.EventId)
            {
                case EventIdAllocTick:
                    AllocationTick?.Invoke(Processors.GcEventProcessor.ProcessTickEvent(e));
                    return;
                case EventIdHeapStats:
                    HeapStats?.Invoke(Processors.GcEventProcessor.ProcessHeapEvent(e));
                    return;
            }

            if (e.EventId == EventIdSuspendEeStart && ((uint) e.Payload![0]! & suspendGcReasons) == 0) return;

            if (_gcPauseEventTimer.TryGetDuration(e, out var pauseDuration) == DurationResult.FinalWithDuration)
            {
                PauseComplete?.Invoke(new GcPauseCompleteNotification(pauseDuration));
                return;
            }

            if (e.EventId == EventIdGcStart)
            {
                CollectionStart?.Invoke(Processors.GcEventProcessor.ProcessCollectionStartEvent(e));
            }

            if (_gcEventTimer.TryGetDuration(e, out var gcDuration, out var gcData) == DurationResult.FinalWithDuration)
            {
                CollectionComplete?.Invoke(new GcCollectionEndNotification(gcData.Generation, gcDuration, gcData.Type));
            }
        }

        #region Fields

        private const int
            EventIdGcStart = 1,
            EventIdGcStop = 2,
            EventIdSuspendEeStart = 9,
            EventIdRestartEeStop = 3,
            EventIdHeapStats = 4,
            EventIdAllocTick = 10;

        private readonly EventPairTimer<uint, GcData> _gcEventTimer = new EventPairTimer<uint, GcData>(
            EventIdGcStart,
            EventIdGcStop,
            x => (uint) x.Payload![0]!,
            x => new GcData((uint) x.Payload![1]!, (RuntimeEventSource.GcType) x.Payload[3]),
            new SamplingRate(1));

        private readonly EventPairTimer<int> _gcPauseEventTimer = new EventPairTimer<int>(
            EventIdSuspendEeStart,
            EventIdRestartEeStop,
            // Suspensions/ Resumptions are always done sequentially so there is no common value to match events on. Return a constant value as the event id.
            x => 1,
            new SamplingRate(1));

        public event Action<GcHeapNotification> HeapStats;
        public event Action<GcAllocationNotification> AllocationTick;
        public event Action<GcCollectionStartNotification> CollectionStart;
        public event Action<GcCollectionEndNotification> CollectionComplete;
        public event Action<GcPauseCompleteNotification> PauseComplete;

        public static Guid EventSourceGuid => RuntimeEventSource.Id;

        public static EventKeywords Keywords => (EventKeywords) RuntimeEventSource.Keywords.GC;

        #endregion
    }
}