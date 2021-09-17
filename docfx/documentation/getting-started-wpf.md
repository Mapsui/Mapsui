
# Mapsui WPF getting started

### Step 1
Start a new WPF application in Visual Studio.

### Step 2
In the package manager console type:
```
PM> Install-Package Mapsui.Native
```

### Step 3
In WpfApplication1.MainWindow.xaml add this in the Grid element:
```
<mapsui:MapControl Name="MyMapControl"></mapsui:MapControl>
```
And add the namespace: ```xmlns:mapsui="clr-namespace:Mapsui.UI.Wpf;assembly=Mapsui.UI.Wpf"```

### Step 4
In WpfApplication1.MainWindow.xaml.cs add in the constructor **after** InitializeComponent():
```
MyMapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer("your-user-agent"));
```
And add the namespaces: ```using Mapsui.Utilities; using Mapsui.Layers; ```

### Step 5
Run!
