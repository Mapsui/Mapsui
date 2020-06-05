# Mapsui.Forms

Mapsui.Forms is a Xamarin.Forms native library for Mapsui. With this
library it is possible to use Mapsui without any renderer.

Mapsui.Forms uses SkaiSharp.Views.Forms to display the map on the
device. This works for iOS, Android, UWP and Mac OS. WPF should be
possible too, but isn't tested.

## Installation

1. Create a normal Xamarin.Forms project
2. Add Mapsui.Forms from NuGet to the packages
3. Add the line `xmlns:mapsui="clr-namespace:Mapsui.UI.Forms;assembly=Mapsui.UI.Forms"`
to the Xaml file
4. Add the Mapsui.Forms view with
````
<mapsui:MapView x:Name="mapView"
    VerticalOptions="FillAndExpand"
    HorizontalOptions="Fill"
    BackgroundColor="Gray" />
````
to the Xaml <ContentPage> part file
5. Add in the code behind the following
````
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
````
6. Now you are ready to run a test
