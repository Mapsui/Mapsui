
# Mapsui Eto getting started

### Step 1
Start a new [Eto.Forms](https://github.com/picoe/Eto/wiki/Quick-Start) application in Visual Studio.

### Step 2
In the package manager console type:
```console
PM> Install-Package Mapsui.Eto -pre
```

### Step 3
In MainForm.cs add this to the class constructor:
```csharp
   var mapControl = new Mapsui.UI.Eto.MapControl();

   mapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

   Content = mapControl;
```

### Step 4
Run it and you should see a map of the world.
