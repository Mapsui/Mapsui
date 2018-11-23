using System;
using System.IO;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Geolocator.Abstractions;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Environment = System.Environment;

namespace Mapsui.Samples.Forms.Droid
{
    [Activity(Label = "Mapsui.Samples.Forms", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        private static string MbTilesLocationOnAndroid => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        protected override void OnCreate(Bundle bundle)
        {
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            MbTilesSample.MbTilesLocation = MbTilesLocationOnAndroid;
            MbTilesHelper.DeployMbTilesFile(s => File.Create(System.IO.Path.Combine(MbTilesLocationOnAndroid, s)));

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new Mapsui.Samples.Forms.App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
