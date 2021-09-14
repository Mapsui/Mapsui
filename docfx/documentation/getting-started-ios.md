# Mapsui iOS Getting Started

### Step 1 

Create new 'Single View App' in Visual Studio

### Step 2

Add package 'Mapsui.Native' to the project (don't forget to enable check at 'Include prereleases')

### Step 3

Open ViewController.cs 

add namespaces:

```
using Mapsui;
using Mapsui.UI.iOS;
using Mapsui.Utilities;
```

add code to ViewDidLoad() method:

```
public override void ViewDidLoad()
{
   base.ViewDidLoad();

   var mapControl = new MapControl(View.Bounds);
   var map = new Map();
   map.Layers.Add(OpenStreetMap.CreateTileLayer("your-user-agent"));
   mapControl.Map = map;
   View = mapControl;
}
```

### Step 4

Run!
