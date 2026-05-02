using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace Mapsui.Samples.Avalonia12.Desktop;

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
                RenderingMode = [
                    Win32RenderingMode.Vulkan,    // Try Vulkan first (fastest if supported)
                    Win32RenderingMode.AngleEgl,  // Try ANGLE (uses DirectX)
                    Win32RenderingMode.Wgl,       // Then try native OpenGL
                    Win32RenderingMode.Software   // Fall back to software if needed
                ],
            })
            .LogToTrace()
            .WithInterFont()
            .UseReactiveUI(_ => {});
}
