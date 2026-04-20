using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaClient.Configuration;
using System.Text;

namespace OpcUaClient.OpcUa
{
    internal sealed class OpcUaSessionService
    {
        private readonly OpcUaClientSettings _settings;
        private readonly ILogger<OpcUaSessionService> _logger;

        public OpcUaSessionService(
            OpcUaClientSettings settings,
            ILogger<OpcUaSessionService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<Session> ConnectAsync(ApplicationConfiguration configuration)
        {
            _logger.LogInformation("Selecting endpoint: {EndpointUrl}", _settings.EndpointUrl);

            bool useSecurity = !string.Equals(
                _settings.Security.SecurityMode,
                "None",
                StringComparison.OrdinalIgnoreCase);

            EndpointDescription? selectedEndpoint =
                await CoreClientUtils.SelectEndpointAsync(
                    configuration,
                    _settings.EndpointUrl,
                    useSecurity,
                    CoreClientUtils.DefaultDiscoverTimeout,
                    telemetry: null!);

            if (selectedEndpoint is null)
            {
                throw new InvalidOperationException(
                    $"No OPC UA endpoint was found for '{_settings.EndpointUrl}'.");
            }

            _logger.LogInformation("Endpoint selected.");
            _logger.LogInformation("Endpoint URL: {EndpointUrl}", selectedEndpoint.EndpointUrl);
            _logger.LogInformation("Security mode: {SecurityMode}", selectedEndpoint.SecurityMode);
            _logger.LogInformation("Security policy: {SecurityPolicy}", selectedEndpoint.SecurityPolicyUri);

            var endpointConfiguration = EndpointConfiguration.Create(configuration);

            var configuredEndpoint = new ConfiguredEndpoint(
                collection: null!,
                description: selectedEndpoint,
                configuration: endpointConfiguration);

            UserIdentity identity = CreateUserIdentity();

            Session session = await Session.Create(
                configuration,
                configuredEndpoint,
                updateBeforeConnect: false,
                checkDomain: false,
                sessionName: _settings.Session.SessionName,
                sessionTimeout: (uint)_settings.Session.SessionTimeoutMs,
                identity,
                preferredLocales: null!);

            if (!session.Connected)
            {
                session.Dispose();

                throw new InvalidOperationException(
                    "The OPC UA session was created but is not connected.");
            }

            _logger.LogInformation("OPC UA session created successfully.");
            _logger.LogInformation("Session name: {SessionName}", session.SessionName);

            return session;
        }

        public void Disconnect(Session? session)
        {
            if (session is null)
            {
                return;
            }

            try
            {
                if (session.Connected)
                {
                    _logger.LogInformation("Closing OPC UA session...");
                    session.Close();
                }
            }
            finally
            {
                session.Dispose();
                _logger.LogInformation("OPC UA session disposed.");
            }
        }

        private UserIdentity CreateUserIdentity()
        {
            if (_settings.Authentication.UseAnonymousUser)
            {
                return new UserIdentity(new AnonymousIdentityToken());
            }

            if (string.IsNullOrWhiteSpace(_settings.Authentication.Username))
            {
                throw new InvalidOperationException(
                    "Username is required when anonymous authentication is disabled.");
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(
                _settings.Authentication.Password ?? string.Empty);

            return new UserIdentity(
                _settings.Authentication.Username,
                passwordBytes);
        }
    }
}
