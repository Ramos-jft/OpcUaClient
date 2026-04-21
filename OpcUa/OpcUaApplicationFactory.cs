using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;
using OpcUaClient.Configuration;

namespace OpcUaClient.OpcUa;

internal sealed class OpcUaApplicationFactory
{
    private const string DirectoryStoreType = "Directory";

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
            SecurityConfiguration = CreateSecurityConfiguration(),
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
        ConfigureCertificateValidation(configuration);

        var application = new ApplicationInstance
        {
            ApplicationName = configuration.ApplicationName,
            ApplicationType = ApplicationType.Client,
            ApplicationConfiguration = configuration
        };

        bool certificateIsValid =
            await application.CheckApplicationInstanceCertificatesAsync(false, 2048);

        if (!certificateIsValid)
        {
            throw new InvalidOperationException(
                "The OPC UA application certificate is invalid or could not be created.");
        }

        _logger.LogInformation("OPC UA application configuration created and validated.");

        return configuration;
    }

    private SecurityConfiguration CreateSecurityConfiguration()
    {
        return new SecurityConfiguration
        {
            ApplicationCertificate = new CertificateIdentifier
            {
                StoreType = DirectoryStoreType,
                StorePath = _settings.Certificates.StorePath,
                SubjectName = $"CN={_settings.ApplicationName}"
            },
            TrustedPeerCertificates = new CertificateTrustList
            {
                StoreType = DirectoryStoreType,
                StorePath = _settings.Certificates.TrustedStorePath
            },
            TrustedIssuerCertificates = new CertificateTrustList
            {
                StoreType = DirectoryStoreType,
                StorePath = _settings.Certificates.TrustedStorePath
            },
            RejectedCertificateStore = new CertificateTrustList
            {
                StoreType = DirectoryStoreType,
                StorePath = _settings.Certificates.RejectedStorePath
            },
            AutoAcceptUntrustedCertificates = _settings.Security.AutoAcceptUntrustedCertificates,
            AddAppCertToTrustedStore = true
        };
    }

    private void ConfigureCertificateValidation(ApplicationConfiguration configuration)
    {
        configuration.CertificateValidator.CertificateValidation += (_, eventArgs) =>
        {
            bool canAcceptCertificate =
                eventArgs.Error.StatusCode == StatusCodes.BadCertificateUntrusted &&
                _settings.Security.AutoAcceptUntrustedCertificates;

            if (!canAcceptCertificate)
            {
                return;
            }

            _logger.LogWarning(
                "Accepting untrusted certificate for local development.");

            eventArgs.Accept = true;
        };
    }

    private void ValidateSettings()
    {
        RequireText(_settings.EndpointUrl, nameof(_settings.EndpointUrl));
        RequireText(_settings.ApplicationName, nameof(_settings.ApplicationName));
        RequireText(_settings.ApplicationUri, nameof(_settings.ApplicationUri));
        RequireText(_settings.ProductUri, nameof(_settings.ProductUri));
        RequirePositive(_settings.Session.SessionTimeoutMs, nameof(_settings.Session.SessionTimeoutMs));
        RequirePositive(_settings.Session.OperationTimeoutMs, nameof(_settings.Session.OperationTimeoutMs));
    }

    private static void RequireText(string value, string settingName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"{settingName} is required in appsettings.json.");
        }
    }

    private static void RequirePositive(int value, string settingName)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException(
                $"{settingName} must be greater than zero.");
        }
    }
}
