using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaClient.Configuration;
using OpcUaClient.Models;
using OpcUaClient.OpcUa;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

OpcUaClientSettings settings = configuration.Get<OpcUaClientSettings>()
    ?? throw new InvalidOperationException("Could not load OPC UA settings from appsettings.json.");

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConfiguration(configuration.GetSection("Logging"))
        .AddConsole();
});

ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

var applicationFactory = new OpcUaApplicationFactory(
    settings,
    loggerFactory.CreateLogger<OpcUaApplicationFactory>());

var sessionService = new OpcUaSessionService(
    settings,
    loggerFactory.CreateLogger<OpcUaSessionService>());

var browseService = new OpcUaBrowseService(
    loggerFactory.CreateLogger<OpcUaBrowseService>());

var readService = new OpcUaReadService(
    loggerFactory.CreateLogger<OpcUaReadService>());

Session? session = null;

try
{
    logger.LogInformation("Starting OPC UA Client.");
    logger.LogInformation("Configured endpoint: {EndpointUrl}", settings.EndpointUrl);

    ApplicationConfiguration applicationConfiguration =
        await applicationFactory.CreateAsync();

    session = await sessionService.ConnectAsync(applicationConfiguration);

    PrintNamespaceTable(session);

    IReadOnlyList<ReferenceDescription> references =
        browseService.BrowseObjectsFolder(session);

    PrintBrowseResult("Objects folder browse result", references);

    NodeId truLaserRootNodeId = NodeId.Parse("ns=2;s=1");

    IReadOnlyList<ReferenceDescription> truLaserRootReferences =
        browseService.BrowseNode(
            session,
            truLaserRootNodeId,
            "TruLaser Root");

    PrintBrowseResult("TruLaser root browse result", truLaserRootReferences);

    NodeId machineNodeId = NodeId.Parse("ns=2;s=30");

    IReadOnlyList<ReferenceDescription> machineDetailsReferences =
        browseService.BrowseNode(
            session,
            machineNodeId,
            "Machine");

    PrintBrowseResult("Machine browse result", machineDetailsReferences);

    NodeId productionPlanNodeId = NodeId.Parse("ns=2;s=2");

    IReadOnlyList<ReferenceDescription> productionPlanReferences =
        browseService.BrowseNode(
            session,
            productionPlanNodeId,
            "ProductionPlan");

    PrintBrowseResult("ProductionPlan browse result", productionPlanReferences);

    IReadOnlyList<OpcNodeValue> values =
        readService.ReadServerStatus(session);

    PrintReadResult(values);

    logger.LogInformation("OPC UA Client finished successfully.");
}
catch (Exception exception)
{
    logger.LogError(exception, "OPC UA Client failed: {Message}", exception.Message);
}
finally
{
    sessionService.Disconnect(session);
}

static void PrintNamespaceTable(Session session)
{
    Console.WriteLine();
    Console.WriteLine("=== Namespace table ===");

    for (uint index = 0; index < session.NamespaceUris.Count; index++)
    {
        Console.WriteLine($"{index}: {session.NamespaceUris.GetString(index)}");
    }
}

static void PrintBrowseResult(
    string title,
    IReadOnlyList<ReferenceDescription> references)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");

    foreach (ReferenceDescription reference in references)
    {
        Console.WriteLine(
            $"- {reference.DisplayName.Text} | NodeClass: {reference.NodeClass} | NodeId: {reference.NodeId}");
    }
}

static void PrintReadResult(IReadOnlyList<OpcNodeValue> values)
{
    Console.WriteLine();
    Console.WriteLine("=== Server status values ===");

    foreach (OpcNodeValue value in values)
    {
        Console.WriteLine($"Node: {value.DisplayName}");
        Console.WriteLine($"  NodeId: {value.NodeId}");
        Console.WriteLine($"  Value: {value.Value}");
        Console.WriteLine($"  DataType: {value.DataType}");
        Console.WriteLine($"  StatusCode: {value.StatusCode}");
        Console.WriteLine($"  SourceTimestamp: {value.SourceTimestamp:O}");
        Console.WriteLine($"  ServerTimestamp: {value.ServerTimestamp:O}");
        Console.WriteLine();
    }
}