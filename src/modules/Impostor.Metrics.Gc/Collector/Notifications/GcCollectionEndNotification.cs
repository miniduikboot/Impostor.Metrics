using System;

namespace Impostor.Metrics.Gc.Collector.Notifications
{
    public readonly struct GcCollectionEndNotification
    {
        public uint Generation { get; }

        public TimeSpan Duration { get; }

        public RuntimeEventSource.GcType Type { get; }

        public GcCollectionEndNotification(uint gen, TimeSpan duration, RuntimeEventSource.GcType type)
        {
            Generation = gen;
            Duration = duration;
            Type = type;
        }
    }
}