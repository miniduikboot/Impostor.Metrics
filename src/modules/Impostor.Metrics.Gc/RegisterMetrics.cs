using System;
using System.Timers;
using Prometheus.Client;

namespace Impostor.Metrics.Gc
{
    internal class RegisterMetrics : IDisposable
    {
        private const string
            AllocBytesPerSecondLabel = "gcalloc",
            AllocationBytesAvg = "gcallocavg",
            GcCollectAverageTicks = "gccolectticksavg",
            GcGen0Size = "gcgen0",
            GcGen0CollectionPerSecond = "gcgen0bytespersec",
            GcGen1Size = "gcgen1",
            GcGen1CollectionPerSecond = "gcgen1bytespersec",
            GcGen2Size = "gcgen2",
            GcGen2CollectionPerSecond = "gcgen2bytespersec",
            GcGen3Size = "gcgen3",
            GcGen3CollectionPerSecond = "gcgen3bytespersec",
            GcPendingFinalizers = "gcpendingfinalizers",
            GcPinnedObjects = "gcpinnedobjects",
            GcPauseAverageTicks = "gcpauseticksavg",
            GcPauseCurrentTicks = "gcpauseticks";

        private readonly Timer _clock;

        private readonly GcDataAggregation _listener;
        private readonly IMetricFactory _metrics;

        private IMetricFamily<IGauge<long>, ValueTuple<string>> _export;

        public RegisterMetrics(IMetricFactory metrics, GcDataAggregation listener)
        {
            _metrics = metrics;
            _listener = listener;
            _clock = new Timer(1000) {AutoReset = true};
            _clock.Elapsed += Tick;
        }

        public void Dispose()
        {
            _clock.Stop();
            _clock.Dispose();
        }

        public void CreateSeries()
        {
            _export = _metrics.CreateGaugeInt64("gc_alloc_collect", "GC Allocation, Collection and pause data.", "type"
            );
            _clock.Start();
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            // update allocations
            _export.WithLabels(AllocBytesPerSecondLabel).Set((long) _listener.AllocationBytesPerSecond);
            _export.WithLabels(AllocationBytesAvg).Set((long) _listener.AllocationAverageSize);

            // update generations
            _export.WithLabels(GcGen0Size).Set((long) _listener.Gen0);
            _export.WithLabels(GcGen1Size).Set((long) _listener.Gen1);
            _export.WithLabels(GcGen2Size).Set((long) _listener.Gen2);
            _export.WithLabels(GcGen3Size).Set((long) _listener.Loh);

            _export.WithLabels(GcGen0CollectionPerSecond).Set((long) _listener.CollectionsPerSecondGen0);
            _export.WithLabels(GcGen1CollectionPerSecond).Set((long) _listener.CollectionsPerSecondGen1);
            _export.WithLabels(GcGen2CollectionPerSecond).Set((long) _listener.CollectionsPerSecondGen2);
            _export.WithLabels(GcGen3CollectionPerSecond).Set((long) _listener.CollectionsPerSecondLoh);

            // update pinned objects and finalizers
            _export.WithLabels(GcPendingFinalizers).Set((long) _listener.PendingFinalizers);
            _export.WithLabels(GcPinnedObjects).Set((long) _listener.PinnedObjects);

            // update pauses
            _export.WithLabels(GcPauseAverageTicks).Set((long) _listener.PauseAverageTicks);
            _export.WithLabels(GcPauseCurrentTicks).Set(_listener.PauseTicksCurrent);
        }
    }
}