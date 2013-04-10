using BruTile.Web;
using Mapsui.Layers;
using System.Windows;

namespace Mapsui.Samples.Wbx
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mapControl.Renderer = new Rendering.WbxRendering.WbxMapRenderer(mapControl.RenderCanvas);
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()));
        }
    }
}
