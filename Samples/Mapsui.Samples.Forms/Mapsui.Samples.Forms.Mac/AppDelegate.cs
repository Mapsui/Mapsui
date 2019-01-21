using AppKit;
using Foundation;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Mapsui.Samples.Forms.Mac
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow window;

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
            // Hack to tell the platform independent samples where the files can be found on iOS.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnMac;
            // Never tested this. PDD.
            MbTilesHelper.DeployMbTilesFile(s => File.Create(Path.Combine(MbTilesLocationOnMac, s)));

            Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        private static string MbTilesLocationOnMac => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
}
