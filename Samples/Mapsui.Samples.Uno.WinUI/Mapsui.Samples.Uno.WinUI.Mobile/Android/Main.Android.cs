using Android.Runtime;
using Com.Nostra13.Universalimageloader.Core;
using Microsoft.UI.Xaml.Media;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Uno.WinUI.Droid;

[Application(
    Label = "@string/ApplicationName",
    Icon = "@mipmap/icon",
    LargeHeap = true,
    HardwareAccelerated = true,
    Theme = "@style/AppTheme"
)]
public class Application : Microsoft.UI.Xaml.NativeApplication
{
    public Application(IntPtr javaReference, JniHandleOwnership transfer)
        : base(() => new App(), javaReference, transfer)
    {
        ConfigureUniversalImageLoader();
    }

    private static void ConfigureUniversalImageLoader()
    {
        // Create global configuration and initialize ImageLoader with this config
        ImageLoaderConfiguration config = new ImageLoaderConfiguration
            .Builder(Context)
            .Build();

        ImageLoader.Instance.Init(config);

        ImageSource.DefaultImageLoader = ImageLoader.Instance.LoadImageAsync;
    }
}
