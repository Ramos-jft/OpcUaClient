using System.Net;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

const string endpointUrl = "opc.tcp://127.0.0.1:50000";

Console.WriteLine("Starting OPC UA connection smoke test...");
Console.WriteLine($"Endpoint: {endpointUrl}");

var config = new ApplicationConfiguration
{
    ApplicationName = "OpcUaClient",
    ApplicationUri = $"urn:{Dns.GetHostName()}:OpcUaClient",
    ProductUri = "urn:qf:opcua-client",
    ApplicationType = ApplicationType.Client,

    SecurityConfiguration = new SecurityConfiguration
    {
        ApplicationCertificate = new CertificateIdentifier
        {
            StoreType = "Directory",
            StorePath = "CertificateStores/OpcUaClient",
            SubjectName = "CN=OpcUaClient"
        },

        TrustedPeerCertificates = new CertificateTrustList
        {
            StoreType = "Directory",
            StorePath = "CertificateStores/Trusted"
        },

        TrustedIssuerCertificates = new CertificateTrustList
        {
            StoreType = "Directory",
            StorePath = "CertificateStores/Issuers"
        },

        RejectedCertificateStore = new CertificateTrustList
        {
            StoreType = "Directory",
            StorePath = "CertificateStores/Rejected"
        },

        AutoAcceptUntrustedCertificates = true,
        AddAppCertToTrustedStore = true
    },

    TransportConfigurations = new TransportConfigurationCollection(),

    TransportQuotas = new TransportQuotas
    {
        OperationTimeout = 15000
    },

    ClientConfiguration = new ClientConfiguration
    {
        DefaultSessionTimeout = 60000
    }
};

await config.Validate(ApplicationType.Client);

config.CertificateValidator.CertificateValidation += (_, e) =>
{
    if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
    {
        Console.WriteLine("Warning: accepting untrusted certificate for local development.");
        e.Accept = true;
    }
};

var application = new ApplicationInstance
{
    ApplicationName = config.ApplicationName,
    ApplicationType = ApplicationType.Client,
    ApplicationConfiguration = config
};

await application.CheckApplicationInstanceCertificates(false, 2048);

Console.WriteLine("Application configuration created.");

var selectedEndpoint = CoreClientUtils.SelectEndpoint(
    config,
    endpointUrl,
    useSecurity: false
);

Console.WriteLine("Endpoint selected:");
Console.WriteLine($"  Url: {selectedEndpoint.EndpointUrl}");
Console.WriteLine($"  SecurityMode: {selectedEndpoint.SecurityMode}");
Console.WriteLine($"  SecurityPolicy: {selectedEndpoint.SecurityPolicyUri}");

var endpointConfiguration = EndpointConfiguration.Create(config);

var endpoint = new ConfiguredEndpoint(
    collection: null,
    description: selectedEndpoint,
    configuration: endpointConfiguration
);

var session = await Session.Create(
    configuration: config,
    endpoint: endpoint,
    updateBeforeConnect: false,
    checkDomain: false,
    sessionName: "Lucas-OpcUaClient-Session",
    sessionTimeout: 60000,
    identity: new UserIdentity(new AnonymousIdentityToken()),
    preferredLocales: null
);

Console.WriteLine("Session created.");
Console.WriteLine($"Connected: {session.Connected}");
Console.WriteLine($"Session name: {session.SessionName}");

Console.WriteLine("Namespace table:");

for (uint i = 0; i < session.NamespaceUris.Count; i++)
{
    Console.WriteLine($"  {i}: {session.NamespaceUris.GetString(i)}");
}

Console.WriteLine("Closing session...");

session.Close();
session.Dispose();

Console.WriteLine("Session closed.");
Console.WriteLine("Smoke test finished.");
