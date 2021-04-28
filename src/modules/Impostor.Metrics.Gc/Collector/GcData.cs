namespace Impostor.Metrics.Gc.Collector
{
    public readonly struct GcData
    {
        public GcData(uint generation, RuntimeEventSource.GcType type)
        {
            Generation = generation;
            Type = type;
        }

        public uint Generation { get; }
        public RuntimeEventSource.GcType Type { get; }
    }
}