using BruTile.Web;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using System.IO;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Mapsui.Samples.Metro
{
    public sealed partial class MainPage
    {
        bool _first = true;

        public MainPage()
        {
            InitializeComponent();
            mapControl.ViewChanged += mapControl_ViewChanged;
        }

        void mapControl_ViewChanged(object sender, Windows.ViewChangedEventArgs e)
        {
            if (!_first) return;
            _first = false;

            // sample: zoom to default area at startup
            var beginPoint = new Point(-4000000, 2000000);
            var endPoint = new Point(4000000, 11000000);
            mapControl.ZoomToBox(beginPoint, endPoint);
            mapControl.Refresh();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()));
            var provider = CreateRandomPointsProvider();
            mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPointLayerWithLabel(provider));
            mapControl.Map.Layers.Add(PointLayerSample.CreateStackedLabelLayer(provider));
            mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPolygonLayer(mapControl.Map.Envelope, 1));
        }

        private Stream GetSymbolBitmapStream()
        {
            // add some sample symbols (resource images) to the map...
            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream(@"Mapsui.Samples.Metro.Resources.Images.ns.png");
            stream.Position = 0;
            return stream;
        }

        private MemoryProvider CreateRandomPointsProvider()
        {
            var randomPoints = PointLayerSample.GenerateRandomPoints(mapControl.Map.Envelope, 200);
            var features = new Features();
            var count = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point };
                feature["Label"] = count.ToString();
                features.Add(feature);
                count++;
            }
            return new MemoryProvider(features);
        }

        private ILayer CreateRandomPointLayerWithLabel(IProvider dataSource, Stream bitmapStream)
        {
            var styles = new StyleCollection
                {
                    new SymbolStyle {Symbol = new Bitmap {Data = bitmapStream}, SymbolRotation = 45.0},
                    new LabelStyle {Text = "TestLabel"}
                };

            return new Layer("pointLayer") { DataSource = dataSource, Style = styles };
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
