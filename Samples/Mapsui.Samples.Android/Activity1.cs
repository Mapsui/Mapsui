using System;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI.Android;

namespace Mapsui.Samples.Android
{
    [Activity(
        Label = "Mapsui.Samples.Android", 
        MainLauncher = true, 
        Icon = "@drawable/icon", 
        ScreenOrientation = ScreenOrientation.Sensor, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            DeployMbTilesFile();
            MbTilesSample.MbTilesLocation = MbTilesLocationOnAndroid;
            
            var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            mapControl.Map = MbTilesSample.CreateMap();
        }

        private void DeployMbTilesFile()
        {
            var path = "Mapsui.Samples.Common.EmbeddedResources.world.mbtiles";
            var assembly = typeof(PointsSample).Assembly;
            using (var image = assembly.GetManifestResourceStream(path))
            {
                if (image == null) throw new ArgumentException("EmbeddedResource not found");
                using (var dest = File.Create(MbTilesLocationOnAndroid))
                {
                    image.CopyTo(dest);
                }
            }
        }

        private static string MbTilesLocationOnAndroid
        {
            get
            {
                var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var path = Path.Combine(folder, "world.mbtiles");
                return path;
            }
        }
    }
}