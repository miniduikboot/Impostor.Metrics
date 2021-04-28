using System.Threading.Tasks;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;

namespace Impostor.Metrics.Gc
{
    [ImpostorPlugin("empi.impostor.metrics.gc", "Impostor Metrics Dashboard - Garbage Collection Statistics", "Alioth Merak", "2")]
    [ImpostorDependency("empi.impostor.metrics", DependencyType.HardDependency)]
    [ImpostorDependency("empi.impostor.metrics", DependencyType.LoadBefore)]
    internal class ImpostorPlugin : IPlugin
    {
        private readonly ILogger<ImpostorPlugin> _logger;

        private readonly RegisterMetrics _register;

        public ImpostorPlugin(ILogger<ImpostorPlugin> logger, RegisterMetrics register)
        {
            _logger = logger;
            _register = register;
        }

        public ValueTask EnableAsync()
        {
            _register.CreateSeries();
            _logger.LogInformation("Impostor.Metrics.Gc: successfully registered exports.");
            return default;
        }

        public ValueTask DisableAsync() => default;

        public ValueTask ReloadAsync() => default;
    }
}