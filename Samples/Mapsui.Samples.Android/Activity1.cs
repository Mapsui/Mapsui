using Android.App;
using Android.Content.PM;
using Android.OS;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Samples.Common;
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
            var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            mapControl.Map = VariousSample.CreateMap();
        }
    }
}

