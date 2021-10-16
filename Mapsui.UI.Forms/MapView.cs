using Mapsui.Layers;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using Mapsui.Widgets;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Extensions;
using Mapsui.Providers;
using Mapsui.Widgets.ButtonWidget;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    /// <summary>
    /// Class, that uses the API of the original Xamarin.Forms MapView
    /// </summary>
    public class MapView : MapControl, INotifyPropertyChanged, IEnumerable<Pin>
    {
        private const string CalloutLayerName = "Callouts";
        private const string PinLayerName = "Pins";
        private const string DrawableLayerName = "Drawables";
        private readonly MemoryLayer _mapCalloutLayer;
        private readonly MemoryLayer _mapPinLayer;
        private readonly MemoryLayer _mapDrawableLayer;
        private ButtonWidget _mapZoomInButton;
        private ButtonWidget _mapZoomOutButton;
        private ButtonWidget _mapMyLocationButton;
        private ButtonWidget _mapNorthingButton;
        private readonly SKPicture _pictMyLocationNoCenter;
        private readonly SKPicture _pictMyLocationCenter;
        private readonly SKPicture _pictZoomIn;
        private readonly SKPicture _pictZoomOut;
        private readonly SKPicture _pictNorthing;

        readonly ObservableRangeCollection<Pin> _pins = new ObservableRangeCollection<Pin>();
        readonly ObservableRangeCollection<Drawable> _drawable = new ObservableRangeCollection<Drawable>();
        readonly ObservableRangeCollection<Callout> _callouts = new ObservableRangeCollection<Callout>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.MapView"/> class.
        /// </summary>
        public MapView()
        {
            MyLocationEnabled = false;
            MyLocationFollow = false;

            IsClippedToBounds = true;
            UseDoubleTap = false;

            MyLocationLayer = new MyLocationLayer(this) { Enabled = true };
            _mapCalloutLayer = new MemoryLayer() { Name = CalloutLayerName, IsMapInfoLayer = true };
            _mapPinLayer = new MemoryLayer() { Name = PinLayerName, IsMapInfoLayer = true };
            _mapDrawableLayer = new MemoryLayer() { Name = DrawableLayerName, IsMapInfoLayer = true };

            // Get defaults from MapControl
            RotationLock = Map.RotationLock;
            ZoomLock = Map.ZoomLock;
            PanLock = Map.PanLock;

            // Add some events to _mapControl
            Viewport.ViewportChanged += HandlerViewportChanged;
            Info += HandlerInfo;
            SingleTap += HandlerTap;
            DoubleTap += HandlerTap;
            LongTap += HandlerLongTap;
            SizeChanged += HandlerSizeChanged;

            TouchMove += (s, e) =>
            {
                RunOnUIThread(() => MyLocationFollow = false);
            };

            // Add MapView layers to Map
            AddLayers();

            // Add some events to _mapControl.Map.Layers
            Map.Layers.Changed += HandleLayersChanged;

            _pictMyLocationNoCenter = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.LocationNoCenter.svg", typeof(MapView)));
            _pictMyLocationCenter = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.LocationCenter.svg", typeof(MapView)));

            _pictZoomIn = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.ZoomIn.svg", typeof(MapView)));
            _pictZoomOut = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.ZoomOut.svg", typeof(MapView)));
            _pictNorthing = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.RotationZero.svg", typeof(MapView)));

            _pins.CollectionChanged += HandlerPinsOnCollectionChanged;
            _drawable.CollectionChanged += HandlerDrawablesOnCollectionChanged;

            _mapCalloutLayer.DataSource = new ObservableCollectionProvider<Callout, IGeometryFeature>(_callouts);
            _mapCalloutLayer.Style = null;  // We don't want a global style for this layer

            _mapPinLayer.DataSource = new ObservableCollectionProvider<Pin, IGeometryFeature>(_pins);
            _mapPinLayer.Style = null;  // We don't want a global style for this layer

            _mapDrawableLayer.DataSource = new ObservableCollectionProvider<Drawable, IGeometryFeature>(_drawable);
            _mapDrawableLayer.Style = null;  // We don't want a global style for this layer
        }

        #region Events

        ///<summary>
        /// Occurs when a pin clicked
        /// </summary>
        public event EventHandler<PinClickedEventArgs> PinClicked;

        /// <summary>
        /// Occurs when selected pin changed
        /// </summary>
        public event EventHandler<SelectedPinChangedEventArgs> SelectedPinChanged;

        /// <summary>
        /// Occurs when map clicked
        /// </summary>
        public event EventHandler<MapClickedEventArgs> MapClicked;

        /// <summary>
        /// Occurs when map long clicked
        /// </summary>
        public event EventHandler<MapLongClickedEventArgs> MapLongClicked;

        #endregion

        #region Bindings

        public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(nameof(SelectedPin), typeof(Pin), typeof(MapView), default(Pin), defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty UniqueCalloutProperty = BindableProperty.Create(nameof(UniqueCallout), typeof(bool), typeof(MapView), false, defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty MyLocationEnabledProperty = BindableProperty.Create(nameof(MyLocationEnabled), typeof(bool), typeof(MapView), false, defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty MyLocationFollowProperty = BindableProperty.Create(nameof(MyLocationFollow), typeof(bool), typeof(MapView), false, defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty UnSnapRotationDegreesProperty = BindableProperty.Create(nameof(UnSnapRotationDegreesProperty), typeof(double), typeof(MapView), default(double));
        public static readonly BindableProperty ReSnapRotationDegreesProperty = BindableProperty.Create(nameof(ReSnapRotationDegreesProperty), typeof(double), typeof(MapView), default(double));
        public static readonly BindableProperty RotationLockProperty = BindableProperty.Create(nameof(RotationLockProperty), typeof(bool), typeof(MapView), default(bool));
        public static readonly BindableProperty ZoomLockProperty = BindableProperty.Create(nameof(ZoomLockProperty), typeof(bool), typeof(MapView), default(bool));
        public static readonly BindableProperty PanLockProperty = BindableProperty.Create(nameof(PanLockProperty), typeof(bool), typeof(MapView), default(bool));
        public static readonly BindableProperty IsZoomButtonVisibleProperty = BindableProperty.Create(nameof(IsZoomButtonVisibleProperty), typeof(bool), typeof(MapView), true);
        public static readonly BindableProperty IsMyLocationButtonVisibleProperty = BindableProperty.Create(nameof(IsMyLocationButtonVisibleProperty), typeof(bool), typeof(MapView), true);
        public static readonly BindableProperty IsNorthingButtonVisibleProperty = BindableProperty.Create(nameof(IsNorthingButtonVisibleProperty), typeof(bool), typeof(MapView), true);
        public static readonly BindableProperty ButtonMarginProperty = BindableProperty.Create(nameof(ButtonMarginProperty), typeof(Thickness), typeof(MapView), new Thickness(20, 20));
        public static readonly BindableProperty ButtonSpacingProperty = BindableProperty.Create(nameof(ButtonSpacingProperty), typeof(double), typeof(MapView), 8.0);
        public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(nameof(ButtonSizeProperty), typeof(double), typeof(MapView), 40.0);
        public static readonly BindableProperty UseDoubleTapProperty = BindableProperty.Create(nameof(UseDoubleTapProperty), typeof(bool), typeof(MapView), default(bool));
        public static readonly BindableProperty UseFlingProperty = BindableProperty.Create(nameof(UseFlingProperty), typeof(bool), typeof(MapView), true);

        #endregion

        #region Properties

        /// <summary>
        /// MyLocation layer
        /// </summary>
        public MyLocationLayer MyLocationLayer { get; }

        /// <summary>
        /// Should my location be visible on map
        /// </summary>
        /// <remarks>
        /// Needs a BeginInvokeOnMainThread to change MyLocationLayer.Enabled
        /// </remarks>
        public bool MyLocationEnabled
        {
            get { return (bool)GetValue(MyLocationEnabledProperty); }
            set { Device.BeginInvokeOnMainThread(() => SetValue(MyLocationEnabledProperty, value)); }
        }

        /// <summary>
        /// Should center of map follow my location
        /// </summary>
        public bool MyLocationFollow
        {
            get { return (bool)GetValue(MyLocationFollowProperty); }
            set { SetValue(MyLocationFollowProperty, value); }
        }

        /// <summary>
        /// Pins on map
        /// </summary>
        public IList<Pin> Pins => _pins;

        /// <summary>
        /// Selected pin
        /// </summary>
        public Pin SelectedPin
        {
            get { return (Pin)GetValue(SelectedPinProperty); }
            set { SetValue(SelectedPinProperty, value); }
        }

        /// <summary>
        /// Single or multiple callouts possible
        /// </summary>
        public bool UniqueCallout
        {
            get { return (bool)GetValue(UniqueCalloutProperty); }
            set { SetValue(UniqueCalloutProperty, value); }
        }

        /// <summary>
        /// List of drawables like polyline and polygon
        /// </summary>
        public IList<Drawable> Drawables => _drawable;

        /// <summary>
        /// Enable rotation with pinch gesture
        /// </summary>
        public bool RotationLock
        {
            get { return (bool)GetValue(RotationLockProperty); }
            set { SetValue(RotationLockProperty, value); }
        }

        /// <summary>
        /// Enable zooming
        /// </summary>
        public bool ZoomLock
        {
            get { return (bool)GetValue(ZoomLockProperty); }
            set { SetValue(ZoomLockProperty, value); }
        }

        /// <summary>
        /// Enable paning
        /// </summary>
        public bool PanLock
        {
            get { return (bool)GetValue(PanLockProperty); }
            set { SetValue(PanLockProperty, value); }
        }

        /// <summary>
        /// Enable zoom buttons
        /// </summary>
        public bool IsZoomButtonVisible
        {
            get { return (bool)GetValue(IsZoomButtonVisibleProperty); }
            set { SetValue(IsZoomButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Enable My Location button
        /// </summary>
        public bool IsMyLocationButtonVisible
        {
            get { return (bool)GetValue(IsMyLocationButtonVisibleProperty); }
            set { SetValue(IsMyLocationButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Enable Northing button
        /// </summary>
        public bool IsNorthingButtonVisible
        {
            get => (bool)GetValue(IsNorthingButtonVisibleProperty);
            set => SetValue(IsNorthingButtonVisibleProperty, value);
        }

        /// <summary>
        /// Margin for buttons
        /// </summary>
        public Thickness ButtonMargin
        {
            get => (Thickness)GetValue(ButtonMarginProperty);
            set => SetValue(ButtonMarginProperty, value);
        }

        /// <summary>
        /// Spacing between buttons
        /// </summary>
        public double ButtonSpacing
        {
            get => (double)GetValue(ButtonSpacingProperty);
            set => SetValue(ButtonSpacingProperty, value);
        }

        /// <summary>
        /// Size of buttons in x- and y-direction
        /// </summary>
        public double ButtonSize
        {
            get => (double)GetValue(ButtonSizeProperty);
            set => SetValue(ButtonSizeProperty, value);
        }

        #endregion

        internal void AddCallout(Callout callout)
        {
            if (!_callouts.Contains(callout))
            {
                if (UniqueCallout)
                    HideCallouts();

                _callouts.Add(callout);

                Refresh();
            }
        }

        internal void RemoveCallout(Callout callout)
        {
            if (_callouts.Contains(callout))
            {
                _callouts.Remove(callout);

                Refresh();
            }
        }

        internal bool IsCalloutVisible(Callout callout)
        {
            return _callouts.Contains(callout);
        }

        /// <summary>
        /// Hide all visible callouts
        /// </summary>
        public void HideCallouts()
        {
            _callouts.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Pin> GetEnumerator()
        {
            return _pins.GetEnumerator();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName.Equals(nameof(MyLocationEnabledProperty)) || propertyName.Equals(nameof(MyLocationEnabled)))
            {
                MyLocationLayer.Enabled = MyLocationEnabled;
                Refresh();
            }

            if (propertyName.Equals(nameof(MyLocationFollowProperty)) || propertyName.Equals(nameof(MyLocationFollow)))
            {
                if (MyLocationFollow)
                {
                    _mapMyLocationButton.Picture = _pictMyLocationCenter;
                    Navigator.CenterOn(MyLocationLayer.MyLocation.ToMapsui());
                }
                else
                {
                    _mapMyLocationButton.Picture = _pictMyLocationNoCenter;
                }

                Refresh();
            }

            if (propertyName.Equals(nameof(RotationLockProperty)) || propertyName.Equals(nameof(RotationLock)))
                Map.RotationLock = RotationLock;

            if (propertyName.Equals(nameof(ZoomLockProperty)) || propertyName.Equals(nameof(ZoomLock)))
                Map.ZoomLock = ZoomLock;

            if (propertyName.Equals(nameof(PanLockProperty)) || propertyName.Equals(nameof(PanLock)))
                Map.PanLock = PanLock;

            if (propertyName.Equals(nameof(IsZoomButtonVisibleProperty)) || propertyName.Equals(nameof(IsZoomButtonVisible)))
            {
                if (_mapZoomInButton != null && _mapZoomOutButton != null)
                {
                    _mapZoomInButton.Enabled = IsZoomButtonVisible;
                    _mapZoomOutButton.Enabled = IsZoomButtonVisible;
                    UpdateButtonPositions();
                }
            }

            if (propertyName.Equals(nameof(IsMyLocationButtonVisibleProperty)) || propertyName.Equals(nameof(IsMyLocationButtonVisible)))
            {
                if (_mapMyLocationButton != null)
                {
                    _mapMyLocationButton.Enabled = IsMyLocationButtonVisible;
                    UpdateButtonPositions();
                }
            }

            if (propertyName.Equals(nameof(IsNorthingButtonVisibleProperty)) || propertyName.Equals(nameof(IsNorthingButtonVisible)))
            {
                if (_mapNorthingButton != null)
                {
                    _mapNorthingButton.Enabled = IsNorthingButtonVisible;
                    UpdateButtonPositions();
                }
            }

            if (propertyName.Equals(nameof(ButtonMarginProperty)) || propertyName.Equals(nameof(ButtonMargin)))
            {
                UpdateButtonPositions();
            }

            if (propertyName.Equals(nameof(ButtonSpacingProperty)) || propertyName.Equals(nameof(ButtonSpacing)))
            {
                UpdateButtonPositions();
            }

            if (propertyName.Equals(nameof(ButtonSizeProperty)) || propertyName.Equals(nameof(ButtonSize)))
            {
                UpdateButtonPositions();
            }

            if (propertyName.Equals(nameof(Map)))
            {
                if (Map != null)
                {
                    // Remove MapView layers
                    RemoveLayers();

                    // Readd them, so that they always on top
                    AddLayers();

                    // Remove widget buttons and readd them
                    RemoveButtons();
                    CreateButtons();

                    // Add event handlers
                    Viewport.ViewportChanged += HandlerViewportChanged;
                    Info += HandlerInfo;
                }
            }
        }

        #region Handlers

        /// <summary>
        /// Viewport of map has changed
        /// </summary>
        /// <param name="sender">Viewport of this event</param>
        /// <param name="e">Event arguments containing what changed</param>
        private void HandlerViewportChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Viewport.Rotation)))
            {
                MyLocationLayer.UpdateMyDirection(MyLocationLayer.Direction, Viewport.Rotation);

                // Update rotationButton
                _mapNorthingButton.Rotation = (float)Viewport.Rotation;
            }

            if (e.PropertyName.Equals(nameof(Viewport.Center)))
            {
                if (MyLocationFollow && !Viewport.Center.Equals(MyLocationLayer.MyLocation.ToMapsui()))
                {
                    //_mapControl.Map.NavigateTo(_mapMyLocationLayer.MyLocation.ToMapsui());
                }
            }
        }

        private void HandleLayersChanged(object sender, LayerCollectionChangedEventArgs args)
        {
            var localRemovedLayers = args.RemovedLayers?.ToList() ?? new List<ILayer>();
            var localAddedLayers = args.AddedLayers?.ToList() ?? new List<ILayer>();

            if (localRemovedLayers.Contains(MyLocationLayer) || localRemovedLayers.Contains(_mapDrawableLayer) || localRemovedLayers.Contains(_mapPinLayer) || localRemovedLayers.Contains(_mapCalloutLayer) || 
                localAddedLayers.Contains(MyLocationLayer) || localAddedLayers.Contains(_mapDrawableLayer) || localAddedLayers.Contains(_mapPinLayer) || localAddedLayers.Contains(_mapCalloutLayer))
                return;

            // Remove MapView layers
            RemoveLayers();

            // Readd them, so that they always on top
            AddLayers();
        }

        private void HandlerPinsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Pin>().Any(pin => pin.Label == null))
                throw new ArgumentException("Pin must have a Label to be added to a map");

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    // Remove old pins from layer
                    if (item is Pin pin)
                    {
                        pin.PropertyChanged -= HandlerPinPropertyChanged;

                        pin.HideCallout();

                        if (SelectedPin != null && SelectedPin.Equals(pin))
                            SelectedPin = null;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is Pin pin)
                    {
                        // Add new pins to layer, so set MapView
                        pin.MapView = this;
                        pin.PropertyChanged += HandlerPinPropertyChanged;
                    }
                }
            }

            Refresh();
        }

        private void HandlerDrawablesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: Do we need any information about this?
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    // Remove old drawables from layer
                    if (item is INotifyPropertyChanged drawable)
                        drawable.PropertyChanged -= HandlerDrawablePropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    // Add new drawables to layer
                    if (item is INotifyPropertyChanged drawable)
                        drawable.PropertyChanged += HandlerDrawablePropertyChanged;
                }
            }

            Refresh();
        }

        private void HandlerInfo(object sender, MapInfoEventArgs e)
        {
            // Click on pin?
            if (e.MapInfo.Layer == _mapPinLayer)
            {
                Pin clickedPin = null;
                var pins = _pins.ToList();

                foreach (var pin in pins)
                {
                    if (pin.IsVisible && pin.Feature.Equals(e.MapInfo.Feature))
                    {
                        clickedPin = pin;
                        break;
                    }
                }

                if (clickedPin != null)
                {
                    SelectedPin = clickedPin;

                    SelectedPinChanged?.Invoke(this, new SelectedPinChangedEventArgs(SelectedPin));

                    var pinArgs = new PinClickedEventArgs(clickedPin, Viewport.ScreenToWorld(e.MapInfo.ScreenPosition).ToForms(), e.NumTaps);

                    PinClicked?.Invoke(this, pinArgs);

                    if (pinArgs.Handled)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            // Check for clicked callouts
            else if (e.MapInfo.Layer == _mapCalloutLayer)
            {
                Callout clickedCallout = null;
                var callouts = _callouts.ToList();

                foreach (var callout in callouts)
                {
                    if (callout.Feature.Equals(e.MapInfo.Feature))
                    {
                        clickedCallout = callout;
                        break;
                    }
                }

                var calloutArgs = new CalloutClickedEventArgs(clickedCallout, 
                    Viewport.ScreenToWorld(e.MapInfo.ScreenPosition).ToForms(),
                    new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.NumTaps);

                clickedCallout?.HandleCalloutClicked(this, calloutArgs);

                e.Handled = calloutArgs.Handled;

                return;
            }
            // Check for clicked drawables
            else if (e.MapInfo.Layer == _mapDrawableLayer)
            {
                Drawable clickedDrawable = null;
                var drawables = _drawable.ToList();

                foreach (var drawable in drawables)
                {
                    if (drawable.IsClickable && drawable.Feature.Equals(e.MapInfo.Feature))
                    {
                        clickedDrawable = drawable;
                        break;
                    }
                }

                var drawableArgs = new DrawableClickedEventArgs(
                    Viewport.ScreenToWorld(e.MapInfo.ScreenPosition).ToForms(),
                    new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.NumTaps);

                clickedDrawable?.HandleClicked(drawableArgs);

                e.Handled = drawableArgs.Handled;
                
                return;
            }
        }

        private void HandlerLongTap(object sender, TappedEventArgs e)
        {
            var args = new MapLongClickedEventArgs(Viewport.ScreenToWorld(e.ScreenPosition).ToForms());

            MapLongClicked?.Invoke(this, args);

            if (args.Handled)
            {
                e.Handled = true;
            }
        }

        private void HandlerTap(object sender, TappedEventArgs e)
        {
            // Close all closable Callouts
            var pins = _pins.ToList();

            e.Handled = false;

            if (Map != null)
            {
                // Check, if we hit a widget
                // Is there a widget at this position
                foreach (var widget in Map.Widgets)
                {
                    if (widget.Enabled && widget.Envelope.Contains(e.ScreenPosition))
                    {
                        if (widget.HandleWidgetTouched(Navigator, e.ScreenPosition))
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }

                // Check, if we hit a drawable
                // Is there a drawable at this position
                var mapInfo = GetMapInfo(e.ScreenPosition);

                if (mapInfo.Feature == null)
                {
                    var args = new MapClickedEventArgs(Viewport.ScreenToWorld(e.ScreenPosition).ToForms(), e.NumOfTaps);

                    MapClicked?.Invoke(this, args);

                    if (args.Handled)
                    {
                        e.Handled = true;
                        return;
                    }

                    // Event isn't handled up to now.
                    // Than look, what we could do.

                    return;
                }

                // A feature is clicked
                var mapInfoEventArgs = new MapInfoEventArgs { MapInfo = mapInfo, Handled = e.Handled, NumTaps = e.NumOfTaps };

                HandlerInfo(sender, mapInfoEventArgs);

                e.Handled = mapInfoEventArgs.Handled;
            }
        }

        private void HandlerPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.RefreshData(Viewport.Extent, Viewport.Resolution, ChangeType.Continuous);

            // Repaint map, because something could have changed
            RefreshGraphics();
        }

        private void HandlerDrawablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.RefreshData(Viewport.Extent, Viewport.Resolution, ChangeType.Continuous);

            // Repaint map, because something could have changed
            RefreshGraphics();
        }

        private void HandlerSizeChanged(object sender, EventArgs e)
        {
            UpdateButtonPositions();
        }

        #endregion

        /// <summary>
        /// Add all layers that MapView uses
        /// </summary>
        private void AddLayers()
        {
            if (!_initialized)
                return;

            // Add MapView layers
            Map.Layers.Add(_mapDrawableLayer, _mapPinLayer, _mapCalloutLayer, MyLocationLayer);
        }

        /// <summary>
        /// Remove all layers that MapView uses
        /// </summary>
        private void RemoveLayers()
        {
            if (!_initialized)
                return;

            // Remove MapView layers
            Map.Layers.Remove(MyLocationLayer, _mapCalloutLayer, _mapPinLayer, _mapDrawableLayer);
        }

        /// <summary>
        /// Get all drawables of layer that contain given point
        /// </summary>
        /// <param name="point">Point to search for in world coordinates</param>
        /// <param name="layer">Layer to search for drawables</param>
        /// <returns>List with all drawables at point, which are clickable</returns>
        private IList<Drawable> GetDrawablesAt(Geometries.Point point, ILayer layer)
        {
            List<Drawable> drawables = new List<Drawable>();

            if (layer.Enabled == false) return drawables;
            if (layer.MinVisible > Viewport.Resolution) return drawables;
            if (layer.MaxVisible < Viewport.Resolution) return drawables;

            if (layer.GetFeaturesInView(layer.Envelope, Viewport.Resolution) is
                IEnumerable<IGeometryFeature> allFeatures)
            {
                // Now check all features, if they are clicked and clickable
                foreach (var feature in allFeatures)
                {
                    if (feature.Geometry.Contains(point))
                    {
                        var drawable = _drawable.Where(f => f.Feature == feature).First();
                        // Take only the clickable object
                        if (drawable.IsClickable)
                            drawables.Add(drawable);
                    }
                }
            }

            // If there more than one drawables found, than reverse, because the top most should be the first
            if (drawables.Count > 1)
                    drawables.Reverse();

            return drawables;
        }

        private void UpdateButtonPositions()
        {
            var newX = Width - ButtonMargin.Right - ButtonSize;
            var newY = ButtonMargin.Top;

            if (IsZoomButtonVisible && _mapZoomInButton != null && _mapZoomOutButton != null)
            {
                _mapZoomInButton.Envelope = new Geometries.BoundingBox(newX, newY, newX + ButtonSize, newY + ButtonSize);
                newY += ButtonSize;
                _mapZoomOutButton.Envelope = new Geometries.BoundingBox(newX, newY, newX + ButtonSize, newY + ButtonSize);
                newY += ButtonSize + ButtonSpacing;
            }

            if (IsMyLocationButtonVisible && _mapMyLocationButton != null)
            {
                _mapMyLocationButton.Envelope = new Geometries.BoundingBox(newX, newY, newX + ButtonSize, newY + ButtonSize);
                newY += ButtonSize + ButtonSpacing;
            }

            if (IsNorthingButtonVisible && _mapNorthingButton != null)
            {
                _mapNorthingButton.Envelope = new Geometries.BoundingBox(newX, newY, newX + ButtonSize, newY + ButtonSize);
            }

            RefreshGraphics();
        }

        private void RemoveButtons()
        {
            var widgets = Map.Widgets.ToList();
            widgets.Remove(_mapZoomInButton);
            widgets.Remove(_mapZoomOutButton); 
            widgets.Remove(_mapMyLocationButton);
            widgets.Remove(_mapNorthingButton);
            Map.Widgets.Clear();
            Map.Widgets.AddRange(widgets);

            RefreshGraphics();
        }

        private void CreateButtons()
        {
            _mapZoomInButton = _mapZoomInButton ?? CreateButton(0, 0, _pictZoomIn, (s, e) => { Navigator.ZoomIn(); e.Handled = true; });
            _mapZoomInButton.Picture = _pictZoomIn;
            _mapZoomInButton.Enabled = IsZoomButtonVisible;
            Map.Widgets.Add(_mapZoomInButton);

            _mapZoomOutButton = _mapZoomOutButton ?? CreateButton(0, 40, _pictZoomOut, (s, e) => { Navigator.ZoomOut(); e.Handled = true; });
            _mapZoomOutButton.Picture = _pictZoomOut;
            _mapZoomOutButton.Enabled = IsZoomButtonVisible;
            Map.Widgets.Add(_mapZoomOutButton);

            _mapMyLocationButton = _mapMyLocationButton ?? CreateButton(0, 88, _pictMyLocationNoCenter, (s, e) => { MyLocationFollow = true; e.Handled = true; });
            _mapMyLocationButton.Picture = _pictMyLocationNoCenter;
            _mapMyLocationButton.Enabled = IsMyLocationButtonVisible;
            Map.Widgets.Add(_mapMyLocationButton);

            _mapNorthingButton = _mapNorthingButton ?? CreateButton(0, 136, _pictNorthing, (s, e) => { RunOnUIThread(() => Navigator.RotateTo(0)); e.Handled = true; });
            _mapNorthingButton.Picture = _pictNorthing;
            _mapNorthingButton.Enabled = IsNorthingButtonVisible;
            Map.Widgets.Add(_mapNorthingButton);

            UpdateButtonPositions();
        }

        private ButtonWidget CreateButton(float x, float y, SKPicture picture, Action<object, WidgetTouchedEventArgs> action)
        {
            var result = new ButtonWidget
            {
                Picture = picture,
                Envelope = new Geometries.BoundingBox(x, y, x + ButtonSize, y + ButtonSize),
                Rotation = 0,
                Enabled = true,
            };
            result.WidgetTouched += (s, e) => action(s, e);
            result.PropertyChanged += (s, e) => RefreshGraphics();

            return result;
        }
    }
}
