using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcUaClient.Configuration
{
    public sealed class OpcUaClientSettings
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string ApplicationUri { get; set; } = string.Empty;
        public string ProductUri { get; set; } = string.Empty;
        public OpcUaSecuritySettings Security { get; set; } = new();
        public OpcUaSessionSettings Session { get; set; } = new();
        public OpcUaAuthenticationSettings Authentication { get; set; } = new();
        public OpcUaCertificateSettings Certificates { get; set; } = new();
    }

    public sealed class OpcUaSecuritySettings
    {
        public string SecurityPolicy { get; set; } = "None";
        public string SecurityMode { get; set; } = "None";
        public bool AutoAcceptUntrustedCertificates { get; set; } = true;
    }

    public sealed class OpcUaSessionSettings
    {
        public string SessionName { get; set; } = "Lucas-OpcUaClient-Session";
        public int SessionTimeoutMs { get; set; } = 60000;
        public int OperationTimeoutMs { get; set; } = 15000;
        public int KeepAliveIntervalMs { get; set; } = 5000;
    }

    public sealed class OpcUaAuthenticationSettings
    {
        public bool UseAnonymousUser { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class OpcUaCertificateSettings
    {
        public string StorePath { get; set; } = "CertificateStores/OpcUaClient";
        public string TrustedStorePath { get; set; } = "CertificateStores/Trusted";
        public string RejectedStorePath { get; set; } = "CertificateStores/Rejected";
    }
}
