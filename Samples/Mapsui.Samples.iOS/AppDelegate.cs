namespace Mapsui.Samples.iOS;

[Register ("AppDelegate")]
public class AppDelegate : UIApplicationDelegate {
    public override UIWindow? Window {
        get;
        set;
    }

    public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
    {
        Mapsui.UI.iOS.MapControl.UseGPU = true;
        // create a new window instance based on the screen size
        Window = new UIWindow (UIScreen.MainScreen.Bounds);

        // create a UIViewController with a single UILabel
        var vc = new ViewController();
        Window.RootViewController = vc;

        // make the window visible
        Window.MakeKeyAndVisible ();

        return true;
    }
}