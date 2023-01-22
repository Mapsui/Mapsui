using System.Runtime.Versioning;
using Microsoft.UI.Xaml;

[assembly: SupportedOSPlatform("windows10.0.18362.0")]
namespace Mapsui.Samples.WinUI;

public partial class App : Application
{
    private Window? mainWindow;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        mainWindow = new MainWindow();
        mainWindow.Activate();
    }
}
