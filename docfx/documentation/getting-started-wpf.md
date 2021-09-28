
# Mapsui WPF getting started

### Step 1
Start a new WPF application in Visual Studio.

### Step 2
In the package manager console type:
```console
PM> Install-Package Mapsui.Wpf -pre
```

### Step 3
In WpfApplication1.MainWindow.xaml add this in the Grid element:
```xml
<mapsui:MapControl Name="MyMapControl"></mapsui:MapControl>
```
And add the namespace: ```xmlns:mapsui="clr-namespace:Mapsui.UI.Wpf;assembly=Mapsui.UI.Wpf"```

### Step 4
In WpfApplication1.MainWindow.xaml.cs add in the constructor **after** InitializeComponent():
```csharp
MyMapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
```
And add the namespaces: ```using Mapsui.Utilities; using Mapsui.Layers; ```

### Step 5
Run it and you should see a map of the world.
