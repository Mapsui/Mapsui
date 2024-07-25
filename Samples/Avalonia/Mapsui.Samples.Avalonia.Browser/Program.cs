using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using Mapsui.Samples.Avalonia;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .UseReactiveUI();

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
