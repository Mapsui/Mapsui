using System;
using System.Diagnostics;
using Mapsui.Logging;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace Mapsui.Samples.Maui
{
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

        private void LogMethod(LogLevel logLevel, string message, Exception? exception)
        {
            Debug.WriteLine($"{logLevel}: {message}, {exception}");
        }
    }
}
