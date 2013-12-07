using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using BruTile.Web;
using Mapsui.Layers;
using Mapsui.UI.Android;

namespace Mapsui.Samples.Android
{
    [Activity(Label = "Mapsui.Samples.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()));
        }
    }
}

