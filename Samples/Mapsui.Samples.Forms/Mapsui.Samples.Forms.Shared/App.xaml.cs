using System;
using System.Diagnostics;
using Mapsui.Logging;

using Xamarin.Forms;

namespace Mapsui.Samples.Forms;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Logger.LogDelegate += LogMethod;

        if (Device.Idiom == TargetIdiom.Phone)
            MainPage = new NavigationPage(new MainPage());
        else
            MainPage = new MainPageLarge();
    }

    protected override void OnStart()
    {
        // Handle when your app starts
    }

    protected override void OnSleep()
    {
        // Handle when your app sleeps
    }

    protected override void OnResume()
    {
        // Handle when your app resumes
    }

    private void LogMethod(LogLevel logLevel, string? message, Exception? exception)
    {
        Debug.WriteLine($"{logLevel}: {message}, {exception}");
    }
}
