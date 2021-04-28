using System.Diagnostics.Tracing;
using Impostor.Metrics.Gc.Collector.Notifications;

namespace Impostor.Metrics.Gc.Collector.Processors
{
    public static partial class GcEventProcessor
    {
        public static GcCollectionStartNotification ProcessCollectionStartEvent(EventWrittenEventArgs e)
        {
            var count = (uint) e.Payload![0]!;
            var generation = (uint) e.Payload[1]!;
            var reason = (RuntimeEventSource.GcReason) e.Payload[2]!;

            return new GcCollectionStartNotification(count, generation, reason);
        }
    }
}