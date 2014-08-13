using Android.App;
using Android.Content.PM;
using Android.OS;
using BruTile.Web;
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
            var mapView = FindViewById<MapControl>(Resource.Id.mapview);
            mapView.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            var lineStringLayer = LineStringSample.CreateLineStringLayer();
            lineStringLayer.Style = CreateLineStringStyle();

            mapView.Map.Layers.Add(lineStringLayer);
            var pointLayer = PointLayerSample.CreateRandomPointLayer(mapView.Map.Envelope);
            pointLayer.Style = CreatePointLayerStyle();
            mapView.Map.Layers.Add(pointLayer);

            mapView.Map.Layers.Add(PointLayerSample.CreateBitmapPointLayer());
        }

        private static IStyle CreatePointLayerStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 1,
                Fill = new Brush(Color.Cyan),
                Outline = { Color = Color.White, Width = 8 },
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

