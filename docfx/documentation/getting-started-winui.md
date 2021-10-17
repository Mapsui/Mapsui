# Mapsui WinUI Getting Started

### Step 1 

Create new 'Blank App. Packaged (WinUI 3 in Desktop)' in Visual Studio

### Step 2

In the package manager console type:
```console
PM> Install-Package Mapsui.WinUI -pre
```

### Step 3

Open MainPage.xaml and add namespace:

```xml
xmlns:winui="using:Mapsui.UI.WinUI"
```

Add MapControl to the Grid:

```xml
<Grid>
  <winui:MapControl x:Name="MyMap" VerticalAlignment="Stretch" HorizontalAlignment="Stretch" />
</Grid>
```


In MainPage.xaml.cs, add namespace:

```csharp
using Mapsui.Utilities;
```

Add code to the constructor:

```csharp
        public MainPage()
        {
            this.InitializeComponent();

            MyMap.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        }

```

### Step 4

Run it and you should see a map of the world.
