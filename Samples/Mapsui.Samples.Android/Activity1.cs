using Android.App;
using Android.OS;
using BruTile.Tms;
using BruTile.Web;
using Mapsui.Layers;
using Mapsui.UI.Android;
using System;
using System.Linq;
using System.Net;

namespace Mapsui.Samples.Android
{
    [Activity(Label = "Mapsui.Samples.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var mapControl = FindViewById<MapView>(Resource.Id.mapcontrol);
            var osmTileSource = new OsmTileSource();
            mapControl.Map.Layers.Add(new TileLayer(osmTileSource, 40, 60));                        
            //mapControl.Map.Layers.Add(LufoTest());
        }

        private TileLayer LufoTest()
        {            
            var client = new WebClient();
            var stream = client.OpenRead(new Uri(@"http://geodata1.nationaalgeoregister.nl/luchtfoto/tms/1.0.0/luchtfoto/EPSG28992"));
            var t = TileMapParser.CreateTileSource(stream);
            return new TileLayer(t);
        }
    }
}

