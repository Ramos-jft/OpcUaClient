# OpcUaClient

Console application in C#/.NET 9 for studying OPC UA client communication.

## Current goal

Connect to a local OPC UA demo server, create a session, browse the server address space, and read basic server status values.

## Requirements

- .NET SDK 9
- Visual Studio 2026 or another editor with .NET support
- A local OPC UA demo server running at the endpoint configured in `appsettings.json`

Default endpoint:

```text
opc.tcp://127.0.0.1:50000
```

## Project structure

```text
OpcUaClient/
├── Program.cs
├── appsettings.json
├── Configuration/
│   └── OpcUaClientSettings.cs
├── Models/
│   └── OpcNodeValue.cs
└── OpcUa/
    ├── OpcUaApplicationFactory.cs
    ├── OpcUaSessionService.cs
    ├── OpcUaBrowseService.cs
    └── OpcUaReadService.cs
```

## How to run

```bash
dotnet restore
dotnet build
dotnet run
```

## What the client currently does

1. Loads settings from `appsettings.json`.
2. Creates and validates the OPC UA application configuration.
3. Selects the configured endpoint.
4. Creates an OPC UA session.
5. Prints the namespace table.
6. Browses `ObjectsFolder`.
7. Browses demo nodes configured in `BrowseNodes`.
8. Reads standard `ServerStatus` values.
9. Closes the session safely.

## Next recommended step

Browse deeper into the `Machine` node, identify nodes with `NodeClass: Variable`, and then read real machine-specific values.
