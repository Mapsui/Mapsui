using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Mapsui.UI.iOS;

namespace Mapsui.Samples.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			window.MakeKeyAndVisible ();

			return true;
		}
	}
}

