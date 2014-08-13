using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Mapsui.Samples.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow _window;
		Mapsui_Samples_iOSViewController _viewController;
		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			_window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			_viewController = new Mapsui_Samples_iOSViewController ();
			_window.RootViewController = _viewController;
			_window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

