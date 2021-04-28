using System.Diagnostics.Tracing;
using Impostor.Metrics.Gc.Collector.Notifications;

namespace Impostor.Metrics.Gc.Collector.Processors
{
    public static partial class GcEventProcessor
    {
        public static GcAllocationNotification ProcessTickEvent(EventWrittenEventArgs e)
        {
            const uint lohHeapFlag = 0x1;
            var allocated = e.Payload![0];
            var lohFlag = e.Payload[1];

            return new GcAllocationNotification(((uint) lohFlag! & lohHeapFlag) == lohHeapFlag, (uint) allocated!);
        }
    }
}