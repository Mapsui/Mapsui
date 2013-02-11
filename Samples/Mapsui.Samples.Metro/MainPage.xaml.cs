using BruTile.Web;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using System.IO;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mapsui.Samples.Metro
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool _first = true;

        public MainPage()
        {
            InitializeComponent();
            mapControl.ViewChanged += mapControl_ViewChanged;
        }

        void mapControl_ViewChanged(object sender, Windows.ViewChangedEventArgs e)
        {
            if (_first)
            {
                _first = false;

                // sample: zoom to default area at startup
                var beginPoint = new Geometries.Point(-4000000, 2000000);
                var endPoint = new Geometries.Point(4000000, 11000000);
                mapControl.ZoomToBox(beginPoint, endPoint);
                mapControl.Refresh();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()));
            
            var pointLayer = PointLayerSample.CreateRandomPointLayer(mapControl.Map.Envelope, 200);
            
            // add some sample symbols (resource images) to the map...
            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream(@"Mapsui.Samples.Metro.Resources.Images.ns.png");
            stream.Position = 0;
            pointLayer.Styles.Clear();
            pointLayer.Styles.Add(new SymbolStyle() { Symbol = new Bitmap() { Data = stream }, SymbolRotation = 45.0 });
            mapControl.Map.Layers.Add(pointLayer);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // sample: zoomin...
            mapControl.ZoomIn();
            mapControl.Refresh();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // sample: zoomin...
            mapControl.ZoomOut();
            mapControl.Refresh();
        }
    }
}
