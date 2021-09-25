# Mapsui UWP Getting Started

### Step 1 

Create new 'Blank App (Universal Windows)' in Visual Studio

### Step 2

```console
Install-Package Mapsui.Uwp -pre
```

### Step 3

Open MainPage.xaml

add namespace:

```xml
xmlns:uwp="using:Mapsui.UI.Uwp"
```

Add MapControl to the Grid:

```xml
<Grid>
    <uwp:MapControl x:Name="MapControl" VerticalAlignment="Stretch" HorizontalAlignment="Stretch" />
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

            MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer("your-user-agent"));
        }

```

### Step 4

Run and you should see a map of the world.
