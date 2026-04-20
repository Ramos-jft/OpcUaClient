using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaClient.Models;

namespace OpcUaClient.OpcUa
{
    internal sealed class OpcUaReadService
    {
        private readonly ILogger<OpcUaReadService> _logger;

        public OpcUaReadService(ILogger<OpcUaReadService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<OpcNodeValue> ReadServerStatus(Session session)
        {
            var nodesToRead = new List<(NodeId NodeId, string DisplayName)>
            {
                (VariableIds.Server_ServerStatus_CurrentTime, "Server current time"),
                (VariableIds.Server_ServerStatus_StartTime, "Server start time"),
                (VariableIds.Server_ServerStatus_State, "Server state"),
                (VariableIds.Server_ServerStatus_BuildInfo_ProductName, "Server product name"),
                (VariableIds.Server_ServerStatus_BuildInfo_SoftwareVersion, "Server software version")
            };

            return ReadNodes(session, nodesToRead);
        }

        private IReadOnlyList<OpcNodeValue> ReadNodes(
            Session session,
            IReadOnlyList<(NodeId NodeId, string DisplayName)> nodes)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (!session.Connected)
            {
                throw new InvalidOperationException(
                    "Cannot read nodes because the OPC UA session is not connected.");
            }

            if (nodes.Count == 0)
            {
                return Array.Empty<OpcNodeValue>();
            }

            _logger.LogInformation("Reading {Count} OPC UA nodes...", nodes.Count);

            var readValueIds = new ReadValueIdCollection();

            foreach ((NodeId nodeId, _) in nodes)
            {
                readValueIds.Add(new ReadValueId
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value
                });
            }

            session.Read(
                requestHeader: null,
                maxAge: 0,
                timestampsToReturn: TimestampsToReturn.Both,
                nodesToRead: readValueIds,
                out DataValueCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, readValueIds);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, readValueIds);

            var values = new List<OpcNodeValue>();

            for (int index = 0; index < results.Count; index++)
            {
                DataValue dataValue = results[index];
                (NodeId nodeId, string displayName) = nodes[index];

                values.Add(new OpcNodeValue
                {
                    NodeId = nodeId,
                    DisplayName = displayName,
                    Value = dataValue.Value,
                    DataType = dataValue.Value?.GetType().Name ?? "null",
                    SourceTimestamp = dataValue.SourceTimestamp,
                    ServerTimestamp = dataValue.ServerTimestamp,
                    StatusCode = dataValue.StatusCode
                });
            }

            _logger.LogInformation("Read completed.");

            return values;
        }
    }
}
