# Smart-X IoT Mesh Gateway (PROG7312 POE Part 1)

Data ingestion and validation gateway for the Smart-X IoT scenario. Built with .NET 10.

## Project layout

- `SmartX.Shared` - class library with the models and data structures shared by both apps
- `SmartX.Api` - ASP.NET Core Web API, receives and stores telemetry
- `SmartX.Client` - Windows Forms app, the dashboard an engineer uses to register sensors and watch telemetry come in

## What each part of the brief maps to

- Generics: `SmartX.Shared/TelemetryPacket.cs`
- Operator overloading: `SmartX.Shared/SensorMeter.cs`, used in the dashboard's "Aggregate Power Load" button
- Jagged arrays into `List<T>`: `SmartX.Shared/TelemetryHistoryStore.cs`, used both server side (telemetry history) and client side (local simulation history)
- Recursion: `SmartX.Shared/DeploymentNode.cs`, used when registering a sensor to check the location path (e.g. `Facility A > Zone 1 > Sub-Zone B`) is a valid nested config
- File upload: `AttachmentsController` on the API, `OpenFileDialog` on the client
- Dynamic engagement feature: anomaly highlighting, sensor rows in the dashboard change colour when a reading is out of range or a sensor stops reporting (matches the Task 1 research report)

## Prerequisites

- .NET 10 SDK
- Windows, to actually run the client (the API can run anywhere, the client needs Windows Forms)

## Restoring and building

From the solution root:

```
dotnet restore
dotnet build
```

## Running the API

```
cd SmartX.Api
dotnet run
```

The API listens on `http://localhost:5154` by default (see `Properties/launchSettings.json`). Leave this running, the client needs it.

## Running the client

Open `Prog7312Initial.sln` in Visual Studio and run `SmartX.Client`, or from a terminal:

```
cd SmartX.Client
dotnet run
```

Make sure the API is already running first, the client points at `http://localhost:5154/` (see `ApiClient` in `MainForm.cs`).

## Using the app

1. Start the API, then start the client
2. Click "Sensor Data Ingestion and Telemetry" on the startup menu
3. The dashboard seeds itself with 5 mock sensors the first time it opens, and starts simulating telemetry every 2 seconds
4. Rows turn pink when a reading is out of range, grey when a sensor stops reporting for 30+ seconds
5. Click "Register New Sensor" to add your own, with MAC address, a location path (e.g. `Facility A > Zone 1 > Sub-Zone B`) and a category, optionally attaching a file
6. Click "Aggregate Power Load" to sum up every power consumption sensor's current reading

The other two menu buttons (Command Stream and Mesh Routing) are disabled on purpose, they come in Part 2 and the final PoE.
