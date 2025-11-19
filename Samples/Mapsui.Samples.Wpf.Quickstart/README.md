# WPF Quickstart Sample

This is a minimal WPF application that follows the quickstart instructions from `docs/general/markdown/index.md` exactly.

## Purpose

This sample validates that the WPF quickstart guide works as documented. It was created by:

1. Creating a new WPF application project
2. Adding a reference to `Mapsui.UI.Wpf` (equivalent to `Install-Package Mapsui.Wpf`)
3. Adding the MapControl code in MainWindow.xaml.cs constructor after InitializeComponent()

## Code Structure

- **App.xaml/cs**: Standard WPF application entry point
- **MainWindow.xaml/cs**: Main window with MapControl initialization as per quickstart guide

## Key Code (from MainWindow.xaml.cs)

```csharp
public MainWindow()
{
    InitializeComponent();

    // Step 3 from quickstart guide: Add MapControl in constructor after InitializeComponent()
    var mapControl = new Mapsui.UI.Wpf.MapControl();
    mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
    Content = mapControl;
}
```

This code matches exactly what the quickstart documentation instructs users to add.

## Building

```bash
dotnet build -c Release
```

## Running

```bash
dotnet run
```

The application should display a world map using OpenStreetMap tiles.

## Validation Status

✅ Builds successfully
✅ Follows quickstart instructions exactly
✅ Uses correct namespace: `Mapsui.UI.Wpf.MapControl`
✅ Uses correct initialization: `Mapsui.Tiling.OpenStreetMap.CreateTileLayer()`
