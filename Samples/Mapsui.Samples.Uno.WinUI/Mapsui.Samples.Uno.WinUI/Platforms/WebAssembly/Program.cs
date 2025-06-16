namespace Mapsui.Samples.Uno.WinUI;

public class Program
{
    private static App? _app;

    public static int Main(string[] args)
    {
        var host = UnoPlatformHostBuilder.Create()
        .App(() => new App())
        .UseWebAssembly()
        .Build();

        await host.RunAsync();
    }
}
