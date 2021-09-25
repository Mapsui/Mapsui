# Mapsui for Xamarin.Forms getting started

### Step 1

Create a new Xamarin.Forms project in Visual Studio. 

File -> New Project -> Mobile App (Xamarin.Forms) -> Select template 'Blank'

### Step 2

Update Xamarin.Forms to the latest version available for all projects.

In the Package Manager Console select the shared project (the project without .Android/.iOS/.UWP extension) as default project and 
type:
```console
PM> Install-Package Mapsui.Forms -pre
```

### Step 3

Open MainPage.Xaml in the head project.

Add the line `xmlns:mapsui="clr-namespace:Mapsui.UI.Forms;assembly=Mapsui.UI.Forms"`
to the Xaml <ContentPage> part file.

Add the Mapsui.Forms view to the StackLayout element.
```xml
<mapsui:MapView x:Name="mapView"
    VerticalOptions="FillAndExpand"
    HorizontalOptions="Fill"
    BackgroundColor="Gray" />
```
    
### Step 4

Open MainPage.Xaml.cs in the head project.

Add the following code in MainPage constructor after InitializeComponent():

```csharp
var map = new Map();
map.Layers.Add(OpenStreetMap.CreateTileLayer());
mapView.Map = map;
```

### Step 5
Run!
