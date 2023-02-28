using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Mapsui.Samples.Forms.Droid;

[Activity(Label = "Mapsui.Samples.Forms", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
{
    protected override void OnCreate(Bundle bundle)
    {
        Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;

        TabLayoutResource = Resource.Layout.Tabbar;
        ToolbarResource = Resource.Layout.Toolbar;

        base.OnCreate(bundle);
        Xamarin.Essentials.Platform.Init(this, bundle);

        global::Xamarin.Forms.Forms.Init(this, bundle);
        LoadApplication(new Mapsui.Samples.Forms.App());
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
    {
        Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}
