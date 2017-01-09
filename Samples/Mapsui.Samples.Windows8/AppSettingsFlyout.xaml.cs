using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.UI.Xaml;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mapsui.Samples.Windows8
{
    public sealed partial class AppSettingsFlyout : SettingsFlyout
    {
        public AppSettingsFlyout()
        {
            InitializeComponent();
            Setup();

            BingAerialRadioButton.Checked += BingAerialRadioButtonOnChecked;
            BingHybridRadioButton.Checked += BingHybridRadioButtonOnChecked;
            BingRoadsRadioButton.Checked += BingRoadsRadioButtonOnChecked;

            EsriWorldPhysicalRadioButton.Checked += EsriWorldPhysicalRadioButtonOnChecked;
            EsriWorldTopoRadioButton.Checked += EsriWorldTopoRadioButtonOnChecked;

            OpenStreetMapRadioButton.Checked += OpenStreetMapRadioButtonOnChecked;
            OpenCycleMapRadioButton.Checked += OpenCycleMapRadioButtonOnChecked;
            OpenCycleMapTransportRadioButton.Checked += OpenCycleMapTransportRadioButtonOnChecked;

            MapQuestRadioButton.Checked += MapQuestRadioButtonOnChecked;
            MapQuestAerialRadioButton.Checked += MapQuestAerialRadioButtonOnChecked;
            MapQuestRoadsAndLabelsRadioButton.Checked += MapQuestRoadsAndLabelsRadioButtonOnChecked;
        }

        private void BingRoadsRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.BingRoads);
        }

        private void BingHybridRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.BingHybrid);
        }

        private void EsriWorldTopoRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.EsriWorldTopo);
        }

        private void EsriWorldPhysicalRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.EsriWorldPhysical);
        }


        private void MapQuestRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.MapQuest);
        }

        private void MapQuestAerialRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.MapQuestAerial);
        }

        private void MapQuestRoadsAndLabelsRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.MapQuestRoadsAndLabels);
        }

        private void OpenStreetMapRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.OpenStreetMap);
        }

        private void OpenCycleMapRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.OpenCycleMap);
        }

        private void OpenCycleMapTransportRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.OpenCycleMapTransport);
        }

        private void BingAerialRadioButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            SetMainTileLayer(KnownTileSource.BingAerial);
        }

        private void SetMainTileLayer(KnownTileSource tileSource)
        {
            var mapControl = GetMapControl();
            if (mapControl != null)
            {
                mapControl.Map.Layers.Clear();
                var mapLayer = new TileLayer(KnownTileSources.Create(tileSource)) { Name = "Map" };
                mapControl.Map.Layers.Add(mapLayer);
                mapControl.Refresh();
            }
        }

        private void Setup()
        {
            var mapControl = GetMapControl();
            if (mapControl != null)
            {
                if (mapControl.Map.Layers.Count > 0)
                {
                    var layer = mapControl.Map.Layers[0] as TileLayer;
                    if (layer != null)
                    {
                        var tileSource = (KnownTileSource)Enum.Parse(typeof(KnownTileSource), layer.TileSource.Name);
                        switch (tileSource)
                        {
                            case KnownTileSource.BingAerial:
                                BingAerialRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.BingHybrid:
                                BingHybridRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.BingRoads:
                                BingRoadsRadioButton.IsChecked = true;
                                break;

                            case KnownTileSource.EsriWorldBoundariesAndPlaces:
                                break;
                            case KnownTileSource.EsriWorldPhysical:
                                EsriWorldPhysicalRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.EsriWorldReferenceOverlay:
                                break;
                            case KnownTileSource.EsriWorldShadedRelief:
                                break;
                            case KnownTileSource.EsriWorldTopo:
                                EsriWorldTopoRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.EsriWorldTransportation:
                                break;

                            case KnownTileSource.MapQuest:
                                MapQuestRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.MapQuestAerial:
                                MapQuestAerialRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.MapQuestRoadsAndLabels:
                                MapQuestRoadsAndLabelsRadioButton.IsChecked = true;
                                break;

                            case KnownTileSource.OpenCycleMap:
                                OpenCycleMapRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.OpenCycleMapTransport:
                                OpenCycleMapTransportRadioButton.IsChecked = true;
                                break;
                            case KnownTileSource.OpenStreetMap:
                                OpenStreetMapRadioButton.IsChecked = true;
                                break;

                            case KnownTileSource.StamenTerrain:
                                break;
                            case KnownTileSource.StamenToner:
                                break;
                            case KnownTileSource.StamenTonerLite:
                                break;
                            case KnownTileSource.StamenWatercolor:
                                break;
                        }
                    }
                }
            }
        }

        private MapControl GetMapControl()
        {
            MapControl result = null;
            var mainFrame = Window.Current.Content as Frame;
            if (mainFrame != null)
            {
                var mainPage = mainFrame.Content as MainPage;
                if (mainPage != null)
                {
                    result = mainPage.FindName("mapControl") as MapControl;
                }
            }

            return result;
        }
    }
}
