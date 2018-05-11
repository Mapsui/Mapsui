# Getting Started 

## Mapsui WPF getting started

This page will show the steps to add a Mapsui map to your WPF application.

### Step 1
Start a new WPF application in Visual Studio.

### Step 2
In the package manager console type:
```
PM> Install-Package Mapsui
```

These assemblies are added to your project:
- BruTile
- ConcurrentHashSet
- Mapsui - The core assembly.
- Mapsui.Geometries
- Mapsui.Rendering.Skia - The alternative renderer used by the MapControl
- Mapsui.Rendering.Xaml - The renderer used by the MapControl
- Mapsui.UI.Wpf
- Newtonsoft.Json
- SkiaSharp
- SkiaSharp.Svg
- SkiaSharp.Views.Desktop
- SkiaSharp.Views.Wpf

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

## Mapsui Android getting started

### Step 1

Create 'Blank App (Android)' in Visual Studio

### Step 2

$ Install-Package Mapsui

### Step 3

In Resources/layout/Main.axml add Mapsui.UI.Android.MapControl:

```
<?xml version="1.0" encoding="utf-8"?>
<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
    android:orientation="vertical"
    android:layout_width="match_parent"
    android:layout_height="match_parent">
  <Mapsui.UI.Android.MapControl
           android:id="@+id/mapcontrol"
           android:layout_width="match_parent"
           android:layout_height="match_parent" />
</LinearLayout>
```
### Step 4

In MainActivity.cs add MapControl after SetContentView(Resource.Layout.Main):

```
protected override void OnCreate(Bundle savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    // Set our view from the "main" layout resource
    SetContentView(Resource.Layout.Main);

    var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);

    var map = new Map();
    map.Layers.Add(OpenStreetMap.CreateTileLayer());
    mapControl.Map = map;
}
```

### Step 5: Run!

## Samples
There are quite a few samples in Mapsui. Run the WPF sample app ([Mapsui.Samples.Wpf](https://github.com/pauldendulk/Mapsui/tree/master/Samples/Mapsui.Samples.Wpf)) in the samples folder to see them in action. Check out the [Samples\Mapsui\Mapsui.Samples.Common\Maps](https://github.com/pauldendulk/Mapsui/tree/master/Samples/Mapsui.Samples.Common/Maps) folder for the source of the samples. Each sample there creates a Map object that corresponds to a sample in the app.

