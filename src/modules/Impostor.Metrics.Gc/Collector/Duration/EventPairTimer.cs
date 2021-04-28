using System;
using System.Diagnostics.Tracing;

namespace Impostor.Metrics.Gc.Collector.Duration
{
    /// <summary>
    ///     To generate metrics, we are often interested in the duration between two events. This class
    ///     helps time the duration between two events.
    /// </summary>
    /// <typeparam name="TId">A type of an identifier present on both events</typeparam>
    /// <typeparam name="TEventData">A struct that represents data of interest extracted from the first event</typeparam>
    public class EventPairTimer<TId, TEventData> where TId : struct where TEventData : struct
    {
        private readonly int _endId;
        private readonly ConcurrentCache<TId, TEventData> _eventStartedAtConcurrentCache;
        private readonly Func<EventWrittenEventArgs, TEventData> _extractData;

        private readonly Func<EventWrittenEventArgs, TId> _extractEventIdFn;

        private readonly SamplingRate _samplingRate;
        private readonly int _startId;

        public EventPairTimer(int startId, int endId,
            Func<EventWrittenEventArgs, TId> extractEventIdFn,
            Func<EventWrittenEventArgs, TEventData> extractData,
            SamplingRate samplingRate,
            ConcurrentCache<TId, TEventData> concurrentCache = null)
        {
            _startId = startId;
            _endId = endId;
            _extractEventIdFn = extractEventIdFn;
            _extractData = extractData;
            _samplingRate = samplingRate;
            _eventStartedAtConcurrentCache =
                concurrentCache ?? new ConcurrentCache<TId, TEventData>(TimeSpan.FromMinutes(1));
        }

        /// <summary>
        ///     Checks if an event is an expected final event- if so, returns true, the duration between it and the start event and
        ///     any data extracted from the first event.
        /// </summary>
        /// <remarks>
        ///     If the event id matches the supplied start event id, then we cache the event until the final event occurs.
        ///     All other events are ignored.
        /// </remarks>
        public DurationResult TryGetDuration(EventWrittenEventArgs e, out TimeSpan duration,
            out TEventData startEventData)
        {
            duration = TimeSpan.Zero;
            startEventData = default;

            if (e.EventId == _startId)
            {
                if (_samplingRate.ShouldSampleEvent())
                {
                    _eventStartedAtConcurrentCache.Set(_extractEventIdFn(e), _extractData(e), e.TimeStamp);
                }

                return DurationResult.Start;
            }

            if (e.EventId != _endId) return DurationResult.Unrecognized;
            var id = _extractEventIdFn(e);

            if (!_eventStartedAtConcurrentCache.TryRemove(id, out startEventData, out var timeStamp))
                return DurationResult.FinalWithoutDuration;

            duration = e.TimeStamp - timeStamp;
            return DurationResult.FinalWithDuration;
        }
    }

    public sealed class EventPairTimer<TId> : EventPairTimer<TId, int> where TId : struct
    {
        public EventPairTimer(int startId, int endId, Func<EventWrittenEventArgs, TId> extractEventIdFn,
            SamplingRate samplingRate, ConcurrentCache<TId, int> concurrentCache = null)
            : base(startId, endId, extractEventIdFn, e => 0, samplingRate, concurrentCache)
        {
        }

        public DurationResult TryGetDuration(EventWrittenEventArgs e, out TimeSpan duration)
        {
            return TryGetDuration(e, out duration, out _);
        }
    }
}