using Microsoft.Extensions.Logging;
using Opc.Ua;
using OpcUaClient.Configuration;

namespace OpcUaClient.OpcUa
{
    public sealed class OpcUaApplicationFactory
    {
        private readonly OpcUaClientSettings _settings;
        private readonly ILogger<OpcUaApplicationFactory> _logger;

        public OpcUaApplicationFactory(OpcUaClientSettings settings, ILogger<OpcUaApplicationFactory> logger)
        {
            _settings = settings;
            _logger = logger;
        }
    }
}
