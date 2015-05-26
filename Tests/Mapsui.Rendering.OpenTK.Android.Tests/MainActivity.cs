using Android.App;
using Android.OS;

namespace Mapsui.Rendering.OpenTK.Android.Tests
{
    [Activity(Label = "Mapsui OpenTK Sample - Tab the screen to go to next sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
        }
    }
}

