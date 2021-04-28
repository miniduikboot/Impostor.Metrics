using System;
using System.Threading;
using System.Timers;
using Impostor.Metrics.Gc.Collector;
using Impostor.Metrics.Gc.Collector.Notifications;
using Timer = System.Timers.Timer;

namespace Impostor.Metrics.Gc
{
    class GcDataAggregation : IDisposable
    {
        private readonly Timer _clock;

        public GcDataAggregation(GcEventListener listener)
        {
            _clock = new Timer(1000) {AutoReset = true, Enabled = true};
            _clock.Elapsed += Tick;

            listener.Events.AllocationTick += Events_AllocationTick;
            listener.Events.CollectionComplete += Events_CollectionComplete;
            listener.Events.HeapStats += Events_HeapStats;
            listener.Events.PauseComplete += Events_PauseComplete;
        }

        public void Dispose()
        {
            _clock.Dispose();
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            AllocationBytesPerSecond = Interlocked.Read(ref _allocationRate);
            CollectionsPerSecondGen0 = Interlocked.Read(ref _g0CollectionRate);
            CollectionsPerSecondGen1 = Interlocked.Read(ref _g1CollectionRate);
            CollectionsPerSecondGen2 = Interlocked.Read(ref _g2CollectionRate);
            CollectionsPerSecondLoh = Interlocked.Read(ref _g3CollectionRate);

            _allocationRate = _g0CollectionRate = _g1CollectionRate = _g2CollectionRate = _g3CollectionRate = 0;
        }

        private void Events_AllocationTick(GcAllocationNotification obj)
        {
            Interlocked.Add(ref _allocationsSizeTotal, obj.SizeBytes);
            Interlocked.Add(ref _allocationRate, obj.SizeBytes);
            Interlocked.Increment(ref _allocationTicksTotal);
        }

        private void Events_CollectionComplete(GcCollectionEndNotification obj)
        {
            Interlocked.Add(ref _collectionsTicksTotal, (ulong) obj.Duration.Ticks);
            Interlocked.Increment(ref _collectionsTotal);

            switch (obj.Generation)
            {
                case 0:
                    Interlocked.Increment(ref _g0CollectionRate);
                    return;
                case 1:
                    Interlocked.Increment(ref _g1CollectionRate);
                    return;
                case 2:
                    Interlocked.Increment(ref _g2CollectionRate);
                    return;
                case 3:
                    Interlocked.Increment(ref _g3CollectionRate);
                    return;
            }
        }

        private void Events_HeapStats(GcHeapNotification obj)
        {
            Gen0 = obj.Gen0Bytes;
            Gen1 = obj.Gen1Bytes;
            Gen2 = obj.Gen2Bytes;
            Loh = obj.LargeObjectHeapBytes;
            PendingFinalizers = obj.FinalizerQueueLength;
            PinnedObjects = obj.PinnedObjectCount;
        }

        private void Events_PauseComplete(GcPauseCompleteNotification obj)
        {
            Interlocked.Add(ref _pauseTicksTotal, (ulong) obj.Duration.Ticks);
            Interlocked.Increment(ref _pausesTotal);

            PauseTicksCurrent = obj.Duration.Ticks;
        }

        #region Allocation

        public ulong AllocationAverageSize =>
            _allocationTicksTotal != 0 ? _allocationsSizeTotal / _allocationTicksTotal : 0;

        private ulong _allocationTicksTotal, _allocationsSizeTotal, _allocationRate;

        public ulong AllocationBytesPerSecond { get; private set; }

        #endregion

        #region Collection

        public ulong CollectionAverageDurationTicks =>
            _collectionsTotal != 0 ? _collectionsTicksTotal / _collectionsTotal : 0;

        private ulong _collectionsTotal, _collectionsTicksTotal;

        #endregion

        #region Generation Size

        public ulong Gen0 { get; private set; }

        public ulong Gen1 { get; private set; }

        public ulong Gen2 { get; private set; }

        public ulong Loh { get; private set; }

        public ulong PendingFinalizers { get; private set; }

        public uint PinnedObjects { get; private set; }

        public ulong CollectionsPerSecondGen0 { get; private set; }

        public ulong CollectionsPerSecondGen1 { get; private set; }

        public ulong CollectionsPerSecondGen2 { get; private set; }

        public ulong CollectionsPerSecondLoh { get; private set; }

        private ulong _g0CollectionRate, _g1CollectionRate, _g2CollectionRate, _g3CollectionRate;

        #endregion

        #region Pause

        public ulong PauseAverageTicks =>
            _pausesTotal != 0 ? _pauseTicksTotal / _pausesTotal : 0;

        private ulong _pausesTotal, _pauseTicksTotal;

        public long PauseTicksCurrent { get; private set; }

        #endregion
    }
}