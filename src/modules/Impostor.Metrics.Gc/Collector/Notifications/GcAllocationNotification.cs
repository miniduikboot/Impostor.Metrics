namespace Impostor.Metrics.Gc.Collector.Notifications
{
    public readonly struct GcAllocationNotification
    {
        public bool IsOnLargeObjectHeap { get; }

        public ulong SizeBytes { get; }

        public GcAllocationNotification(bool loh, ulong size)
        {
            IsOnLargeObjectHeap = loh;
            SizeBytes = size;
        }
    }
}