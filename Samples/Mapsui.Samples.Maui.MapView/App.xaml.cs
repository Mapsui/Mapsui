using System.Diagnostics;
using Mapsui.Logging;
using Application = Microsoft.Maui.Controls.Application;
using LogLevel = Mapsui.Logging.LogLevel;
#pragma warning disable IDISP004

namespace Mapsui.Samples.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Logger.LogDelegate += LogMethod;
    }

    private void LogMethod(LogLevel logLevel, string message, Exception? exception)
    {
        Debug.WriteLine($"{logLevel}: {message}, {exception}");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            return new Window(new NavigationPage(new MainPage()));

        return new Window(new MainPageLarge());
    }
}
