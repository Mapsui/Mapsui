# Mapsui QuickStart - Avalonia

A minimal Avalonia application demonstrating Mapsui integration based on the [Mapsui v5 QuickStart documentation](https://mapsui.com/v5/).

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or as specified in the repository's `global.json`)
- On Linux: Install required dependencies for Avalonia and SkiaSharp:
  ```bash
  # Ubuntu/Debian
  sudo apt-get install -y libx11-dev libfontconfig1-dev
  ```

## Building

From the repository root:

```bash
# Restore dependencies
dotnet restore Samples/QuickStart.Avalonia/QuickStart.Avalonia.csproj

# Build the project
dotnet build Samples/QuickStart.Avalonia/QuickStart.Avalonia.csproj
```

Or build via the solution filter:

```bash
dotnet build Mapsui.Avalonia.slnf
```

## Running

```bash
dotnet run --project Samples/QuickStart.Avalonia/QuickStart.Avalonia.csproj
```

The application will display a window with an interactive OpenStreetMap view.

## What This Sample Demonstrates

- Setting up a minimal Avalonia application with Mapsui
- Adding the Mapsui `MapControl` to an Avalonia window
- Loading OpenStreetMap tiles as a base layer

## Project Structure

- `Program.cs` - Application entry point and Avalonia configuration
- `App.axaml` / `App.axaml.cs` - Application definition with Fluent theme
- `MainWindow.axaml` / `MainWindow.axaml.cs` - Main window containing the MapControl with OpenStreetMap layer

## Additional Resources

- [Mapsui Documentation](https://mapsui.com/)
- [Mapsui GitHub Repository](https://github.com/Mapsui/Mapsui)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
