using AppKit;

namespace Mapsui.Samples.Uno.WinUI.macOS;

static class MainClass
{
    static void Main(string[] args)
    {
        NSApplication.Init();
        NSApplication.SharedApplication.Delegate = new App();
        NSApplication.Main(args);
    }
}
