using BruTile.Web;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BruTile;
using System.Reflection;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mapsui.Samples.Metro
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()));
            //!!!mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPolygonLayer(new OsmTileSource().Extent.ToBoundingBox()));
            
            var pointLayer = PointLayerSample.CreateRandomPointLayer(mapControl.Map.Envelope, 25);
            Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream(@"Mapsui.Samples.Metro.Resources.Images.ns.png");
            stream.Position = 0;
            pointLayer.Styles.Clear();
            pointLayer.Styles.Add(new SymbolStyle() { Symbol = new Bitmap() { Data = stream }, SymbolRotation = 45.0 });
            mapControl.Map.Layers.Add(pointLayer);

            mapControl.ZoomToFullEnvelope();
            mapControl.Refresh();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomIn();
            mapControl.Refresh();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomOut();
            mapControl.Refresh();
        }
    }
}
