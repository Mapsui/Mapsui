using System;
using Android.Runtime;
using Uno.UI.Hosting;

namespace Mapsui.Samples.Uno5.Droid;

[global::Android.App.ApplicationAttribute(
    Label = "@string/ApplicationName",
    Icon = "@mipmap/icon",
    LargeHeap = true,
    HardwareAccelerated = true,
    Theme = "@style/Theme.App.Starting"
)]
public class Application : global::Uno.UI.Runtime.Android.NativeApplication
{
    static Application()
    {
        App.InitializeLogging();
    }

    public Application(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override UnoPlatformHost CreateHost() =>
        UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseAndroid()
            .Build();

}

