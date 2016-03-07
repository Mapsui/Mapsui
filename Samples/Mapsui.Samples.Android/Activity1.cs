using Android.App;
using Android.Content.PM;
using Android.OS;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
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
            mapControl.Map.Layers.Add(LineStringSample.CreateLineStringLayer(CreateLineStringStyle()));
            mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPointLayer(mapControl.Map.Envelope,
                style: PointLayerSample.CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png")));
            mapControl.Map.Layers.Add(PointLayerSample.CreateBitmapPointLayer());

            mapControl.Map.Viewport.RenderResolutionMultiplier = 2;
        }
        

        private static IStyle CreatePointLayerStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 1,
                Fill = new Brush(Color.Cyan),
                Outline = { Color = Color.White, Width = 4 },
                Line = null
            };
        }

        private static IStyle CreateLineStringStyle()
        {
            return new VectorStyle
            {
                Fill = null,
                Outline = null,
                Line = { Color = Color.Red, Width = 4 }
            };
        }
    }
}

