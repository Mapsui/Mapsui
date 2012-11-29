using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BruTile.Web;
using Mapsui;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;
using MapControl = Mapsui.Windows.MapControl;
using Mapsui.Providers;

namespace Mapsui.Silverlight
{
    public partial class GUIOverlay : UserControl
    {
        public MapControl mapControl; //todo: remove
        bool isMenuDown;

        public GUIOverlay()
        {
            InitializeComponent();
            hideMenu.Completed += hideMenu_Completed;
            showMenu.Completed += showMenu_Completed;
            Loaded += GUIOverlay_Loaded;
            SizeChanged += GUIOverlay_SizeChanged;
        }

        internal void SetMap(MapControl mapControl)
        {
            this.mapControl = mapControl;
            mapControl.ErrorMessageChanged += map_ErrorMessageChanged;

            mapControl.Map = CreateMap();

            FillLayerList(mapControl.Map);
            if (mapControl.Map.Envelope != null)
            {
                var center = mapControl.Map.Envelope.GetCentroid();
                mapControl.Viewport.Center = new Mapsui.Geometries.Point(center.X, center.Y);
                mapControl.Viewport.Resolution = 10000;
            }
        }

        private Map CreateMap()
        {
            var map = new Map();
            var osmLayer = new TileLayer(new OsmTileSource()) {LayerName = "OSM"};
            map.Layers.Add(osmLayer);
            var pointProvider = new MemoryProvider(GenerateRandomPoints(osmLayer.Envelope));
            var pointLayer = new Layer("Points") {DataSource = pointProvider};
            pointLayer.Styles.Add(new Mapsui.Styles.Style());
            map.Layers.Add(pointLayer);
            return map;
        }

        private static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox box)
        {
            var random = new Random();
            var result = new List<IGeometry>();
            for (int i = 0; i < 30; i++)
            {
                result.Add(new Mapsui.Geometries.Point(random.NextDouble() * box.Width + box.Left, random.NextDouble() * box.Height - box.Top));
            }
            return result;
        }

        void FillLayerList(Map map)
        {
            var random = new Random(DateTime.Now.Second);
                        
            bool firstButton = true;

            foreach (ILayer layer in map.Layers)
            {
                if (layer is GroupTileLayer)
                {
                    foreach (ILayer subLayer in (layer as GroupTileLayer).Layers)
                    {
                        var checkBox = new CheckBox();
                        checkBox.Margin = new Thickness(10, 0, 0, 0);
                        checkBox.Click += checkBox_Click;
                        checkBox.Name = random.Next().ToString(); //subLayer.LayerName;
                        checkBox.Content = subLayer.LayerName;
                        checkBox.Tag = subLayer;
                        checkBox.Margin = new Thickness(4);
                        checkBox.FontSize = 12;
                        checkBox.IsChecked = true;

                        layerList.Children.Add(checkBox);
                        
                        if (firstButton)
                        {
                            checkBox.IsChecked = true;
                            firstButton = false;
                        }
                    }
                }
            }
        }

        void checkBox_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement)) return;
            var layer = ((sender as FrameworkElement).Tag as ILayer);
            layer.Enabled = !layer.Enabled;
            mapControl.Clear();
            mapControl.OnViewChanged(true);
        }

        void map_ErrorMessageChanged(object sender, EventArgs e)
        {
            Error.Text = mapControl.ErrorMessage;
            AnimateOpacity(errorBorder, 0.75, 0, 8000);
        }

        public static void AnimateOpacity(UIElement target, double from, double to, int duration)
        {
            target.Opacity = 0;
            var animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new TimeSpan(0, 0, 0, 0, duration);

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            var storyBoard = new Storyboard();
            storyBoard.Children.Add(animation);
            storyBoard.Begin();
        }

        void SetClip()
        {
            var geom = new RectangleGeometry();
            geom.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
            Clip = geom;
        }

        #region layer handling


        #endregion

        #region menu animation events

        void showMenu_Completed(object sender, EventArgs e)
        {
            isMenuDown = true;
        }

        private void showBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            menuShowHideOn.Visibility = Visibility.Collapsed;
        }

        private void showBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            menuShowHideOn.Visibility = Visibility.Visible;
        }

        private void ShowMenuStart()
        {
            showBtn.Visibility = Visibility.Collapsed;
            showMenu.Begin();
        }

        private void showBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowMenuStart();
        }

        void hideMenu_Completed(object sender, EventArgs e)
        {
            menuShowHideOn.Visibility = Visibility.Visible;
            showBtn.Visibility = Visibility.Visible;
            isMenuDown = false;
        }

        private void hideBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            menuShowHideOff.Visibility = Visibility.Collapsed;
        }

        private void hideBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            menuShowHideOff.Visibility = Visibility.Visible;
        }

        private void hideBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            hideMenu.Begin();
        }

        # endregion

        #region event handlers

        void GUIOverlay_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetClip();
        }

        void GUIOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            SetClip();
            GoTo.SetGui(this);
        }

        private void buttonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomIn();
        }

        private void buttonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomOut();
        }

        private void btnLayers_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuDown)
                ShowMenuStart();
        }

        private void btnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
        }

        private void btnBbox_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomToBoxMode = true;
        }

        private void buttonMaxExtend_Click(object sender, RoutedEventArgs e)
        {
            var extent = mapControl.Map.Envelope;
            mapControl.ZoomToBox(new Mapsui.Geometries.Point(extent.MinX, extent.MinY), new Mapsui.Geometries.Point(extent.MaxX, extent.MaxY));
        }

        private void btnGoto_Click(object sender, RoutedEventArgs e)
        {
            GoTo.Visibility = Visibility.Visible;
            GoTo.ShowGoTo.Begin();

            if (Application.Current.Host.Content.IsFullScreen)
            {
                GoTo.errorGrid.Visibility = Visibility.Visible;
            }
            else
            {
                GoTo.errorGrid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region TopTooltip

        private void buttonZoomIn_MouseEnter(object sender, MouseEventArgs e)
        {
            txtTooltipTop.Text = "Zoom In";
            ShowTopTooltip.Begin();
        }

        private void buttonZoomIn_MouseLeave(object sender, MouseEventArgs e)
        {
            HideTopTooltip.Begin();
        }

        private void buttonZoomOut_MouseEnter(object sender, MouseEventArgs e)
        {
            txtTooltipTop.Text = "Zoom Out";
            ShowTopTooltip.Begin();
        }

        private void buttonZoomOut_MouseLeave(object sender, MouseEventArgs e)
        {
            HideTopTooltip.Begin();
        }

        private void buttonMaxExtend_MouseEnter(object sender, MouseEventArgs e)
        {
            txtTooltipTop.Text = "Max Extend";
            ShowTopTooltip.Begin();
        }

        private void buttonMaxExtend_MouseLeave(object sender, MouseEventArgs e)
        {
            HideTopTooltip.Begin();
        }

        #endregion

        #region lowertooltip

        private void btnLayers_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowLowerTooltip.Begin();
            txtTooltipBottom.Text = "Layers";
        }

        private void btnFullscreen_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowLowerTooltip.Begin();
            txtTooltipBottom.Text = "Fullscreen";
        }

        private void btnBbox_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowLowerTooltip.Begin();
            txtTooltipBottom.Text = "bbox zoom";
        }

        private void btnHand_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowLowerTooltip.Begin();
            txtTooltipBottom.Text = "Pan";
        }

        private void btnGoto_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowLowerTooltip.Begin();
            txtTooltipBottom.Text = "Go To";
        }

        private void lower_MouseLeave(object sender, MouseEventArgs e)
        {
            HideLowerTooltip.Begin();
        }

        #endregion
    }
}