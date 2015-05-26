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
            var tileLayer = CreateTileLayer();
            mapControl.Map.Layers.Add(tileLayer);

            mapControl.Map.Layers.Add(LineStringSample.CreateLineStringLayer(CreateLineStringStyle()));

            var pointLayer = PointLayerSample.CreateRandomPointLayer(mapControl.Map.Envelope);
            pointLayer.Style = CreatePointLayerStyle();
            mapControl.Map.Layers.Add(pointLayer);

            mapControl.Map.Layers.Add(PointLayerSample.CreateBitmapPointLayer());

            mapControl.Map.Viewport.RenderResolutionMultiplier = 2;
        }

        private static TileLayer CreateTileLayer()
        {
            var tileLayer = new TileLayer(KnownTileSources.Create(KnownTileSource.EsriWorldReferenceOverlay))
            {
                Name = "OSM"
            };
            return tileLayer;
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

