using System.IO;
using Plugin.Permissions;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Mapsui.Samples.Common.Maps;
using Environment = System.Environment;
using Mapsui.Samples.Common.Utilities;

namespace Mapsui.Samples.Forms.Droid;

[Activity(Label = "Mapsui.Samples.Forms", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
{
    protected override void OnCreate(Bundle bundle)
    {
        Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;

        TabLayoutResource = Resource.Layout.Tabbar;
        ToolbarResource = Resource.Layout.Toolbar;

        base.OnCreate(bundle);

        global::Xamarin.Forms.Forms.Init(this, bundle);
        LoadApplication(new Mapsui.Samples.Forms.App());
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
    {
        PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}
