﻿using System.Runtime.Versioning;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

[assembly: SupportedOSPlatform("windows10.0.18362")]

namespace Mapsui.Samples.Maui.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    [SupportedOSPlatform("windows10.0.18362")]
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
