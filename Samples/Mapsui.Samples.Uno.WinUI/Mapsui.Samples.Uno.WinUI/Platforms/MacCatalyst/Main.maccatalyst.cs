using UIKit;

namespace Mapsui.Samples.Uno.WinUI.MacCatalyst;

public class EntryPoint
{
    // This is the main entry point of the application.
    public static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(App));
    }
}
