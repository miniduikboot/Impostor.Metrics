namespace Impostor.Metrics.Gc.Collector.Notifications
{
    public readonly struct GcHeapNotification
    {
        public ulong Gen0Bytes { get; }

        public ulong Gen1Bytes { get; }

        public ulong Gen2Bytes { get; }

        public ulong LargeObjectHeapBytes { get; }

        public ulong FinalizerQueueLength { get; }

        public uint PinnedObjectCount { get; }

        public GcHeapNotification(ulong g0, ulong g1, ulong g2, ulong loh, ulong fql, uint npo)
        {
            Gen0Bytes = g0;
            Gen1Bytes = g1;
            Gen2Bytes = g2;
            LargeObjectHeapBytes = loh;
            FinalizerQueueLength = fql;
            PinnedObjectCount = npo;
        }
    }
}