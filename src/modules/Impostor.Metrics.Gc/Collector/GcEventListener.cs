using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Impostor.Metrics.Gc.Collector
{
    internal class GcEventListener : EventListener, IDisposable
    {
        private readonly EventLevel _level;

        private readonly GcEventProcessor _processor;

        public GcEventListener(EventLevel level)
        {
            _level = level;
            _processor = new GcEventProcessor();
            EventSourceCreated += CreateEventSource;
        }

        public IGcEventSource Events => _processor;

        public override void Dispose()
        {
            EventSourceCreated -= CreateEventSource;
            base.Dispose();
        }

        private void CreateEventSource(object sender, EventSourceCreatedEventArgs e)
        {
            var es = e.EventSource;
            if (es!.Guid != RuntimeEventSource.Id) return;
            EnableEvents(es, _level, GcEventProcessor.Keywords, new Dictionary<string, string>());
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            try
            {
                if (eventData.EventSource.Guid == GcEventProcessor.EventSourceGuid)
                {
                    _processor.ProcessEvent(eventData);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}