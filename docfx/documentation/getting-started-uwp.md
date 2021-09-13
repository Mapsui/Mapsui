# Mapsui UWP Getting Started

### Step 1 

Create new 'Blank App (Universal Windows)' in Visual Studio

### Step 2

Add package 'Mapsui.Native' to the project (don't forget to enable check at 'Include prereleases')

### Step 3

Open MainPage.xaml

add namespace:

```
xmlns:uwp="using:Mapsui.UI.Uwp"
```

In MainPage.xaml.cs, add namespace:

```
using Mapsui.Utilities;
```

Add code to the constructor:

```
        public MainPage()
        {
            this.InitializeComponent();

            MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer("your-user-agent"));
        }

```

### Step 4

Run!
