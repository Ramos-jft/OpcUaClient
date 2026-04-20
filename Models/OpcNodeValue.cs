using Opc.Ua;

namespace OpcUaClient.Models
{
    internal sealed class OpcNodeValue
    {
        public NodeId NodeId { get; init; } = NodeId.Null;
        public string DisplayName { get; init; } = string.Empty;
        public object? Value { get; init; }
        public string DataType { get; init; } = string.Empty;
        public DateTime SourceTimestamp { get; init; }
        public DateTime ServerTimestamp { get; init; }
        public StatusCode StatusCode { get; init; }
    }
}