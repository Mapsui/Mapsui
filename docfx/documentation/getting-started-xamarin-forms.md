# Mapsui for Xamarin.Forms getting started

Mapsui can be used on Xamarin.Forms with the [Mapsui.Forms](https://www.nuget.org/packages/Mapsui.Forms/) nuget package.

Mapsui.Forms is a 'native' Xamarin.Forms implementation, meaning it does
not use Xamarin.Forms Renderers around platform specific components but implements 
a Xamarin.Forms implementation instead.

Mapsui.Forms uses SkiaSharp.Views.Forms to display the map on the
device. This works for iOS, Android, UWP and Mac OS. On WPF there still 
is an issue with placing controls over the Map.

## Installation

### Step 1. 

Create a normal Xamarin.Forms project

### Step 2. 

Add Mapsui.Forms from NuGet to the packages

### Step 3. 

Add the line `xmlns:mapsui="clr-namespace:Mapsui.UI.Forms;assembly=Mapsui.UI.Forms"`
to the Xaml file

### Step 4. 

Add the Mapsui.Forms view with
```xml
<mapsui:MapView x:Name="mapView"
    VerticalOptions="FillAndExpand"
    HorizontalOptions="Fill"
    BackgroundColor="Gray" />
```
to the Xaml <ContentPage> part file
    
### Step 5. 

Add in the code behind the following

```csharp
var map = new Map
{
    CRS = "EPSG:3857",
    Transformation = new MinimalTransformation()
};

var attribution = new BruTile.Attribution("© OpenStreetMap contributors",
    "http://www.openstreetmap.org/copyright");
var tileSource = new HttpTileSource(new GlobalSphericalMercator(),
    "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
    new[] { "a", "b", "c" }, name: "OpenStreetMap",
    attribution: attribution);
var tileLayer = new TileLayer(tileSource) { Name = "OpenStreetMap" };

map.Layers.Add(tileLayer);
map.Widgets.Add(new Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment = Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Bottom });

mapView.Map = map;
```

### Step 6.
Now you are ready to run a test
