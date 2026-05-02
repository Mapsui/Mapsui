using System.Collections.ObjectModel;
using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using ReactiveUI.Avalonia;

namespace Mapsui.Samples.Avalonia12.Android;

[Activity(
    Label = "Mapsui.Samples.Avalonia12",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .With(new AndroidPlatformOptions
            {
                // Use Software Rendering for broadest device compatibility
                RenderingMode = new ReadOnlyCollection<AndroidRenderingMode>(new[] { AndroidRenderingMode.Software }),
            })
            .WithInterFont()
            .UseReactiveUI(_ => {});
    }
}
