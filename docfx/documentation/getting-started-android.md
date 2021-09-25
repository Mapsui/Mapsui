# Mapsui Android getting started

### Step 1

Create 'Blank App (Android)' in Visual Studio

### Step 2

In the package manager console type:
```
PM> Install-Package Mapsui.Android -pre
```

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

Add the following using statements:

```
using Mapsui;
using Mapsui.Utilities;
using Mapsui.UI.Android;
```


### Step 5:
Run it and you should see a map of the world.
