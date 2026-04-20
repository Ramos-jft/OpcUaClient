using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUaClient.OpcUa
{
    internal sealed class OpcUaBrowseService
    {
        private readonly ILogger<OpcUaBrowseService> _logger;

        public OpcUaBrowseService(ILogger<OpcUaBrowseService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<ReferenceDescription> BrowseObjectsFolder(Session session)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (!session.Connected)
            {
                throw new InvalidOperationException(
                    "Cannot browse because the OPC UA session is not connected.");
            }

            _logger.LogInformation("Browsing OPC UA Objects folder...");

            session.Browse(
                requestHeader: null,
                view: null,
                nodeToBrowse: ObjectIds.ObjectsFolder,
                maxResultsToReturn: 0,
                browseDirection: BrowseDirection.Forward,
                referenceTypeId: ReferenceTypeIds.HierarchicalReferences,
                includeSubtypes: true,
                nodeClassMask: (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method),
                out byte[] continuationPoint,
                out ReferenceDescriptionCollection references);

            if (continuationPoint is { Length: > 0 })
            {
                _logger.LogWarning(
                    "The browse operation returned a continuation point. This beginner version does not process BrowseNext yet.");
            }

            _logger.LogInformation(
                "Browse completed. References found: {Count}",
                references.Count);

            return references.ToList();
        }

        public IReadOnlyList<ReferenceDescription> BrowseNode(
    Session session,
    NodeId nodeId,
    string nodeLabel)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(nodeId);

            if (!session.Connected)
            {
                throw new InvalidOperationException(
                    "Cannot browse because the OPC UA session is not connected.");
            }

            _logger.LogInformation(
                "Browsing OPC UA node '{NodeLabel}' with NodeId '{NodeId}'...",
                nodeLabel,
                nodeId);

            session.Browse(
                requestHeader: null,
                view: null,
                nodeToBrowse: nodeId,
                maxResultsToReturn: 0,
                browseDirection: BrowseDirection.Forward,
                referenceTypeId: ReferenceTypeIds.HierarchicalReferences,
                includeSubtypes: true,
                nodeClassMask: (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method),
                out byte[] continuationPoint,
                out ReferenceDescriptionCollection references);

            if (continuationPoint is { Length: > 0 })
            {
                _logger.LogWarning(
                    "The browse operation returned a continuation point. This beginner version does not process BrowseNext yet.");
            }

            _logger.LogInformation(
                "Browse completed for '{NodeLabel}'. References found: {Count}",
                nodeLabel,
                references.Count);

            return references.ToList();
        }
    }
}
