using System.Diagnostics.Tracing;
using Impostor.Metrics.Gc.Collector.Notifications;

namespace Impostor.Metrics.Gc.Collector.Processors
{
    public static partial class GcEventProcessor
    {
        public static GcHeapNotification ProcessHeapEvent(EventWrittenEventArgs e)
        {
            var gen0 = (ulong) e.Payload![0]!;
            var gen1 = (ulong) e.Payload[2]!;
            var gen2 = (ulong) e.Payload[4]!;
            var loh = (ulong) e.Payload[6]!; // Large object heap
            var fql = (ulong) e.Payload[9]!; // Finalize queue length
            var npo = (uint) e.Payload[10]!; // Number of pinned objects

            return new GcHeapNotification(gen0, gen1, gen2, loh, fql, npo);
        }
    }
}