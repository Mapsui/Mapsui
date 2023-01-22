using AppKit;
using Foundation;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Utilities;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Mapsui.Samples.Forms.Mac;

[Register("AppDelegate")]
public class AppDelegate : FormsApplicationDelegate
{
    readonly NSWindow window;

    public AppDelegate()
    {
        var style = NSWindowStyle.Closable | NSWindowStyle.Titled | NSWindowStyle.Resizable;
        var rect = new CoreGraphics.CGRect(0, 0, 800, 600);

        window = new NSWindow(rect, style, NSBackingStore.Buffered, false);

        window.Title = "Mapsui Samples for Mac";
        window.TitleVisibility = NSWindowTitleVisibility.Hidden;
    }

    public override NSWindow MainWindow => window;

    public override void DidFinishLaunching(NSNotification notification)
    {
        Xamarin.Forms.Forms.Init();
        LoadApplication(new App());

        base.DidFinishLaunching(notification);
    }

    public override void WillTerminate(NSNotification notification)
    {
        // Insert code here to tear down your application
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            window.Dispose();
        }
    }

    private static string MbTilesLocationOnMac => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
}
