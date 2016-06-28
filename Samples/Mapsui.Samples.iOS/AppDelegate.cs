using BruTile.Predefined;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Mapsui.Samples.iOS
{
	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class OpenGLESSampleAppDelegate : UIApplicationDelegate
	{
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			// If you have defined a view, add it here:
			// window.AddSubview (navigationController.View);

			glView.Map.Layers.Add(new Layers.TileLayer(KnownTileSources.Create()));

			glView.Run(60.0);

			var width = window.Frame.Width;
			var height = window.Frame.Height;
			glView.Frame = new CGRect(0, 0, width, height);
			window.MakeKeyAndVisible();

			return true;
		}

		public override void OnResignActivation(UIApplication application)
		{
			glView.Stop();
			glView.Run(5.0);
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated(UIApplication application)
		{
			glView.Stop();
			glView.Run(60.0);
		}
	}
}

