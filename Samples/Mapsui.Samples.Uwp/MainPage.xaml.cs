using Windows.UI.Xaml.Controls;
using BruTile;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Layers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Mapsui.Samples.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            MapControl.Map.Layers.Add(new TileLayer(CreateOsmTileSource()));
        }

        private ITileSource CreateOsmTileSource()
        {
            return new HttpTileSource(new GlobalSphericalMercator(0, 18),
                "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                new[] {"a", "b", "c"}, name: "OSM");
        }
    }
}
