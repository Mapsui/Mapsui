namespace Mapsui.Samples.Wpf.Quickstart;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // Step 3 from quickstart guide: Add MapControl in constructor after InitializeComponent()
        var mapControl = new Mapsui.UI.Wpf.MapControl();
        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        Content = mapControl;
    }
}
