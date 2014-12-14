using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Primitives;
using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Mapsui.UI.Xaml;

namespace Mapsui.Samples.Windows8
{
    public sealed partial class MainPage
    {
        private Geolocator _loc;

        public MainPage()
        {
            InitializeComponent();
            mapControl.Viewport.Resolution = 2000;
            mapControl.Viewport.Center = new Point(0, 0);
            mapControl.ViewChanged += OnMapControlViewChanged;
            mapControl.Map.Layers.LayerAdded += OnMapLayerAdded;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));

            GetGeoLocator();
            
            UpdateZoomControls();

            //var provider = CreateRandomPointsProvider();
            //mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPointLayerWithLabel(provider));
            //mapControl.Map.Layers.Add(PointLayerSample.CreateStackedLabelLayer(provider));
            //mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPolygonLayer(mapControl.Map.Envelope, 1));
        }

        private void UpdateZoomControls()
        {
            var countOfZoomLevels = mapControl.Map.Resolutions.Count;
            MapZoomSlider.Minimum = 0;
            MapZoomSlider.Maximum = countOfZoomLevels - 1;
            var currentRes = mapControl.Map.Viewport.Resolution;
            var pos = mapControl.Map.Resolutions.IndexOf(currentRes);
            MapZoomSlider.Value = pos;
            SliderZoomOutButton.IsEnabled = MenuZoomOutButton.IsEnabled = pos > 0;
            SliderZoomInButton.IsEnabled = MenuZoomInButton.IsEnabled = pos < (countOfZoomLevels - 1);
        }

        private void SetLocation(double longitude, double latitude)
        {
            Point sphericalLocation = SphericalMercator.FromLonLat(longitude, latitude);
            mapControl.Viewport.Center = sphericalLocation;
            if (Dispatcher.HasThreadAccess)
            {
                mapControl.Refresh();
            }
            else
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => mapControl.Refresh());
            }
        }

        private Geolocator GetGeoLocator()
        {
            if (_loc == null)
            {
                _loc = new Geolocator();
                _loc.PositionChanged += OnGeolocatorPositionChanged;
            }

            return _loc;
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

        private void OnGeolocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if (sender.LocationStatus == PositionStatus.Ready)
            {
                SetLocation(
                    args.Position.Coordinate.Point.Position.Longitude,
                    args.Position.Coordinate.Point.Position.Latitude);
            }
        }

        private void OnMapControlViewChanged(object sender, ViewChangedEventArgs e)
        {
            UpdateZoomControls();
        }

        private void OnMapLayerAdded(ILayer layer)
        {
            UpdateZoomControls();
        }

        private void OnZoomInButtonClicked(object sender, RoutedEventArgs e)
        {
            // sample: zoomin...
            mapControl.ZoomIn();
            mapControl.Refresh();
            MapZoomSlider.Value = mapControl.Map.Resolutions.IndexOf(mapControl.Map.Viewport.Resolution);
        }

        private void OnZoomOutButtonClicked(object sender, RoutedEventArgs e)
        {
            // sample: zoomin...
            mapControl.ZoomOut();
            mapControl.Refresh();
            MapZoomSlider.Value = mapControl.Map.Resolutions.IndexOf(mapControl.Map.Viewport.Resolution);
        }

        private void OnRefreshMapAppBarButtonClicked(object sender, RoutedEventArgs e)
        {
            mapControl.Refresh();
        }

        private void OnMapZoomSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            mapControl.Map.Viewport.Resolution = mapControl.Map.Resolutions[(int)e.NewValue];
            mapControl.Refresh();
            UpdateZoomControls();
        }

        private void OnGoToCurrentLocationMenoButtonClicked(object sender, RoutedEventArgs e)
        {
            var geoLocator = GetGeoLocator();

            if (geoLocator.LocationStatus == PositionStatus.Ready)
            {
                var task = geoLocator.GetGeopositionAsync().AsTask();
                Task.WaitAll(task);
                if (task.IsCompleted)
                {
                    SetLocation(
                        task.Result.Coordinate.Longitude,
                        task.Result.Coordinate.Latitude);
                }
            }
        }
    }
}
