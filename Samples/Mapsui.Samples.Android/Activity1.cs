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

            mapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()) { Name = "OSM" });
            mapControl.Map.Layers.Add(LineStringSample.CreateLineStringLayer(LineStringSample.CreateLineStringStyle()));
            mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPointLayer(mapControl.Map.Envelope,
                style: PointLayerSample.CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png")));
            mapControl.Map.Layers.Add(PointLayerSample.CreateBitmapPointLayer());

            mapControl.Map.Viewport.RenderResolutionMultiplier = 2;
        }
    }
}

