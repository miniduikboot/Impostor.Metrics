using System;

namespace Impostor.Metrics.Gc.Collector.Notifications
{
    public readonly struct GcPauseCompleteNotification
    {
        public TimeSpan Duration { get; }

        public GcPauseCompleteNotification(TimeSpan duration) => Duration = duration;
    }
}