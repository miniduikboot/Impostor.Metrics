namespace Impostor.Metrics.Gc.Collector.Notifications
{
    public readonly struct GcCollectionStartNotification
    {
        public uint Count { get; }

        public uint Generation { get; }

        public RuntimeEventSource.GcReason Reason { get; }

        public GcCollectionStartNotification(uint count, uint generation, RuntimeEventSource.GcReason reason)
        {
            Count = count;
            Generation = generation;
            Reason = reason;
        }
    }
}