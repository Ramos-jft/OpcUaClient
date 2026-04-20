using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;
using OpcUaClient.Configuration;

namespace OpcUaClient.OpcUa
{
    public sealed class OpcUaApplicationFactory
    {
        private readonly OpcUaClientSettings _settings;
        private readonly ILogger<OpcUaApplicationFactory> _logger;

        public OpcUaApplicationFactory(
            OpcUaClientSettings settings,
            ILogger<OpcUaApplicationFactory> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<ApplicationConfiguration> CreateAsync()
        {
            ValidateSettings();

            var configuration = new ApplicationConfiguration
            {
                ApplicationName = _settings.ApplicationName,
                ApplicationUri = _settings.ApplicationUri,
                ProductUri = _settings.ProductUri,
                ApplicationType = ApplicationType.Client,

                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = _settings.Certificates.StorePath,
                        SubjectName = $"CN={_settings.ApplicationName}"
                    },

                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = _settings.Certificates.TrustedStorePath
                    },

                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = _settings.Certificates.TrustedStorePath
                    },

                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = _settings.Certificates.RejectedStorePath
                    },

                    AutoAcceptUntrustedCertificates = _settings.Security.AutoAcceptUntrustedCertificates,
                    AddAppCertToTrustedStore = true
                },

                TransportConfigurations = new TransportConfigurationCollection(),

                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = _settings.Session.OperationTimeoutMs
                },

                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = _settings.Session.SessionTimeoutMs
                }
            };

            await configuration.ValidateAsync(ApplicationType.Client);

            configuration.CertificateValidator.CertificateValidation += (_, eventArgs) =>
            {
                if (eventArgs.Error.StatusCode == StatusCodes.BadCertificateUntrusted &&
                    _settings.Security.AutoAcceptUntrustedCertificates)
                {
                    _logger.LogWarning(
                        "Accepting untrusted certificate for local development.");

                    eventArgs.Accept = true;
                }
            };

            var application = new ApplicationInstance
            {
                ApplicationName = configuration.ApplicationName,
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = configuration
            };

            await application.CheckApplicationInstanceCertificatesAsync(false, 2048);

            _logger.LogInformation("OPC UA application configuration created and validated.");

            return configuration;
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.EndpointUrl))
            {
                throw new InvalidOperationException(
                    "EndpointUrl is required in appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(_settings.ApplicationName))
            {
                throw new InvalidOperationException(
                    "ApplicationName is required in appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(_settings.ApplicationUri))
            {
                throw new InvalidOperationException(
                    "ApplicationUri is required in appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(_settings.ProductUri))
            {
                throw new InvalidOperationException(
                    "ProductUri is required in appsettings.json.");
            }

            if (_settings.Session.SessionTimeoutMs <= 0)
            {
                throw new InvalidOperationException(
                    "SessionTimeoutMs must be greater than zero.");
            }

            if (_settings.Session.OperationTimeoutMs <= 0)
            {
                throw new InvalidOperationException(
                    "OperationTimeoutMs must be greater than zero.");
            }
        }
    }
}
