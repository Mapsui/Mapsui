
# Mapsui WPF getting started

This page will show the steps to add a Mapsui map to your WPF application.

### Step 1
Start a new WPF application in Visual Studio.

### Step 2
In the package manager console type:
```
PM> Install-Package Mapsui
```

### Step 3
In WpfApplication1.MainWindow.xaml add this in the Grid element:
```
<xaml:MapControl Name="MyMapControl"></xaml:MapControl>
```
And add the namespace: ```xmlns:xaml="clr-namespace:Mapsui.UI.Wpf;assembly=Mapsui.UI.Wpf"```

### Step 4
In WpfApplication1.MainWindow.xaml.cs add in the constructor:
```
MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));
```
And add the namespaces: ```using BruTile.Predefined; using Mapsui.Layers; ```

### Step 5
Run!
