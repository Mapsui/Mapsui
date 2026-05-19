using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace Mapsui.Tools.ImageComparison;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [
                    Win32RenderingMode.Vulkan,
                    Win32RenderingMode.AngleEgl,
                    Win32RenderingMode.Wgl,
                    Win32RenderingMode.Software,
                ],
            })
            .LogToTrace()
            .WithInterFont()
            .UseReactiveUI(_ => { });
}
