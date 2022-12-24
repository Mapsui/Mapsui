using Mapsui.Samples.Maui.View;
using Mapsui.Samples.Maui.ViewModel;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Mapsui.Samples.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                // Without the line below the app will crash with this exception: "Catastrophic failure (0x8000FFFF (E_UNEXPECTED))".
                // and without the 'true' parameter Android will crash with this exception: "Microsoft.Maui.Platform.HandlerNotFoundException: 'Handler not found for view SkiaSharp.Views.Maui.Controls.SKGLView.'"
                .UseSkiaSharp(true)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();


            return builder.Build();
        }
    }
}