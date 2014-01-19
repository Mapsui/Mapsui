using Android.App;
using Android.Content.PM;
using Android.OS;
using BruTile.Web;
using Mapsui.Layers;
using Mapsui.UI.Android;

namespace Mapsui.Samples.Android
{
    [Activity(Label = "Mapsui.Samples.Android", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Nosensor)]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            var mapView = FindViewById<MapView>(Resource.Id.mapview);
            mapView.Map.Layers.Add(new TileLayer(new OsmTileSource()));
        }
    }
}

