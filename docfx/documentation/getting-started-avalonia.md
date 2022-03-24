
# Mapsui Avalonia getting started

### Step 1
Create a new Avalonia application in your IDE. You may need to [install Avalonia IDE support](https://docs.avaloniaui.net/docs/getting-started/ide-support).

### Step 2
In the package manager console type:
```console
PM> Install-Package Mapsui.Avalonia -pre
```

### Step 3
In MainWindow.axaml.cs add this to the constructor **after** InitializeComponent()::
```csharp
   var mapControl = new Mapsui.UI.Avalonia.MapControl();

   mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

   Content = mapControl;
```

### Step 4
Run it and you should see a map of the world.
