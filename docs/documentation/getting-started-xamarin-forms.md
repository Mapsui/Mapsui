# Mapsui for Xamarin.Forms getting started

### Step 1

Create a normal Xamarin.Forms project

### Step 2

In the package manager console type:
```console
PM> Install-Package Mapsui.Forms -pre
```

### Step 3

Add the line `xmlns:mapsui="clr-namespace:Mapsui.UI.Forms;assembly=Mapsui.UI.Forms"`
to the Xaml file

### Step 4

Add the Mapsui.Forms view with
```xml
<mapsui:MapView x:Name="mapView"
    VerticalOptions="FillAndExpand"
    HorizontalOptions="Fill"
    BackgroundColor="Gray" />
```
to the Xaml <ContentPage> part file.
    
Nest the MapView element inside a container, this child element needs to be placed inside a parent Layout 
for the view to be correctly setup and attached to the code behind, for instance,

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:d="http://xamarin.com/schemas/2014/forms/design"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:mapsui="clr-namespace:Mapsui.UI.Forms;assembly=Mapsui.UI.Forms"
             mc:Ignorable="d">

    <StackLayout>
        <mapsui:MapView x:Name="mapView"
         VerticalOptions="FillAndExpand"
         HorizontalOptions="Fill"
         BackgroundColor="Gray" />
    </StackLayout>

</ContentPage>
```
the Xaml file should look similar to this after this step.

### Step 5

Add in the code behind the following

```csharp
var map = new Map
{
    CRS = "EPSG:3857",
    Transformation = new MinimalTransformation()
};

var tileLayer = OpenStreetMap.CreateTileLayer();

map.Layers.Add(tileLayer);
map.Widgets.Add(new Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment = Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Bottom });

mapView.Map = map;
```

### Step 6
Run it and you should see a map of the world.
