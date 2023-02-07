
# Mapsui Avalonia getting started

### Step 0
Install the Avalonia templates:
```console
dotnet new install Avalonia.Templates
```

### Step 1
Create a new Avalonia project:
```console
dotnet new avalonia.app -o MyApp
```

### Step 2
Add the Mapsui.Avalonia nuget package:
```console
dotnet add MyApp package Mapsui.Avalonia
```

### Step 3
In MainWindow.axaml.cs add this to the constructor **after** InitializeComponent():
```csharp
var mapControl = new Mapsui.UI.Avalonia.MapControl();
mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
Content = mapControl;
```

### Step 4
Run it and you should see a map of the world.
```console
cd MyApp
dotnet run
```
