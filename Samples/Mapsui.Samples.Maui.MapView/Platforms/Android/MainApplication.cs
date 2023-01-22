using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using System;
using System.Runtime.Versioning;
using Microsoft.Maui.Hosting;

[assembly: SupportedOSPlatform("android31.0")]

namespace Mapsui.Samples.Maui;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
