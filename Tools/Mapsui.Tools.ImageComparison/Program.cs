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
                // No Vulkan: Avalonia 12.1's Vulkan backend crashes with SkiaSharp 4 (access violation in
                // VulkanDevice.Create). Restore it once Avalonia fixes that.
                RenderingMode = [
                    Win32RenderingMode.AngleEgl,
                    Win32RenderingMode.Wgl,
                    Win32RenderingMode.Software,
                ],
            })
            .LogToTrace()
            .WithInterFont()
            .UseReactiveUI(_ => { });
}
