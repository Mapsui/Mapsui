using Mapsui.UI.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Mapsui.Samples.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // GPU does not work currently on MAUI
            MapControl.UseGPU = false;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseSkiaSharp(true)
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => {
                    fonts.AddFont("OpenSansRegular.ttf", "OpenSansRegular");
                });

            return builder.Build();
        }
    }
}