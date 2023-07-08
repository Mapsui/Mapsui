using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using System.Collections.ObjectModel;

namespace Mapsui.Samples.Avalonia.Android;

[Activity(
    Label = "Mapsui.Samples.Avalonia.Android",
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
                // Only use Software Rendering
                RenderingMode = new ReadOnlyCollection<AndroidRenderingMode>(new[]{AndroidRenderingMode.Software}),
            })
            .WithInterFont()
            .UseReactiveUI();
    }
}
