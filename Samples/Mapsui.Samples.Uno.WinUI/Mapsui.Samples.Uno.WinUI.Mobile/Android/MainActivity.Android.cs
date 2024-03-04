using Android.Views;

namespace Mapsui.Samples.Uno.WinUI;

[Activity(
        MainLauncher = true,
        ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
        WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden
    )]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
}
