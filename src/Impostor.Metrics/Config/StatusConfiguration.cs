using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Impostor.Metrics.Config
{
    public class StatusConfiguration
    {
        public const string Section = "MetricsExports";

        public ushort Port { get; set; } = 8080;

        public bool EnableCpuStatus { get; set; } = true;

        public bool EnableMemoryStatus { get; set; } = true;

        public bool EnableEventStatus { get; set; } = true;

        public bool EnableGameStatus { get; set; } = true;

        public bool EnableThreadStatus { get; set; } = true;

        public string ExportEndPoint { get; set; } = "/metrics";

        public string Host { get; set; } = "localhost";

        public void ValidateConfig()
        {
            Trace.Assert(ExportEndPoint.StartsWith("/"), "Http endpoint must start with a forward slash (\"/\")!");
            Trace.Assert(!ExportEndPoint.EndsWith("/"), "Http endpoint cannot end with a slash.");

            var condition = EnableCpuStatus || EnableEventStatus || EnableGameStatus || EnableThreadStatus;
            Trace.Assert(condition, "You must have at least one metric export.");

            Trace.Assert(Port!=0, "Port cannot be 0");

            Trace.Assert(!string.IsNullOrWhiteSpace(Host), "Host cannot be null or empty.");
            Trace.Assert(IPAddress.TryParse(Host, out _) || Dns.GetHostAddresses(Host).Length > 0, $"Invalid host interface: \"{Host}\"");
        }

    }
}
