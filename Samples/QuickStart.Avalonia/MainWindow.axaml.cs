using Avalonia.Controls;
using Mapsui.Tiling;

namespace QuickStart.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Add the OpenStreetMap layer following the Mapsui v5 QuickStart guidance
        // See: https://mapsui.com/v5/
        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
    }
}
