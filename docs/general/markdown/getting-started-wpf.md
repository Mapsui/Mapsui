
# Mapsui WPF getting started

### Step 1
Start a new WPF application in Visual Studio.

### Step 2
In the package manager console type:
```console
PM> Install-Package Mapsui.Wpf -pre
```

### Step 4
In MainWindow.xaml.cs add in the constructor **after** InitializeComponent():

```csharp
var mapControl = new Mapsui.UI.Wpf.MapControl();

mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

Content = mapControl;
```

### Step 4
Run it and you should see a map of the world.
