using System;
using System.Diagnostics;
using Mapsui.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Application = Microsoft.Maui.Controls.Application;
using LogLevel = Mapsui.Logging.LogLevel;

namespace Mapsui.Samples.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Logger.LogDelegate += LogMethod;

        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            MainPage = new NavigationPage(new MainPage());
        else
            MainPage = new MainPageLarge();
    }

    private void LogMethod(LogLevel logLevel, string message, Exception? exception)
    {
        Debug.WriteLine($"{logLevel}: {message}, {exception}");
    }
}
