using Mapsui.UI.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiApp3;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MapControl.UseGPU = false;
        var builder = MauiApp.CreateBuilder();
        builder
            .UseSkiaSharp()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
