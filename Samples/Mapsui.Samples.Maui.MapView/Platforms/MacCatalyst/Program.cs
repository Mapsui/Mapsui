using System.Runtime.Versioning;
using UIKit;

[assembly: SupportedOSPlatform("maccatalyst15.0")]

namespace Mapsui.Samples.Maui;

public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
