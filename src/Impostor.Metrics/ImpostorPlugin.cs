using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;

namespace Impostor.Metrics
{
    [ImpostorPlugin("empi.impostor.metrics", "Impostor Metrics Dashboard", "Alioth Merak", "2")]
    public class ImpostorPlugin : IPlugin
    {
        private readonly ILogger<ImpostorPlugin> _logger;

        private readonly PrometheusServer _server;

        public ImpostorPlugin(ILogger<ImpostorPlugin> logger, PrometheusServer server)
        {
            this._logger = logger;
            this._server = server;
        }

        public ValueTask EnableAsync()
        {
            _server.Start();
            return default;
        }

        public ValueTask DisableAsync()
        {
            _server.Stop();
            return default;
        }

        public ValueTask ReloadAsync()
        {
            return default;
        }
    }
}
