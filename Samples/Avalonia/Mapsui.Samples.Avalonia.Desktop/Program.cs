using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.ReactiveUI;

namespace Mapsui.Samples.Avalonia.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                // Not in v11. Is there an alternative?: EnableMultitouch = true,
                // Egl does not work on all platforms for example not on Windows on Arm so only use software rendering for now.
                // This was in earlier versions AllowEglInitialization = false
                RenderingMode = new ReadOnlyCollection<Win32RenderingMode>(new[]{Win32RenderingMode.Software}),
            })
            .LogToTrace()
            .WithInterFont()
            .UseReactiveUI();
}
