using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Svg.Skia;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;
using Mapsui.Widgets.Button;
using Mapsui.Widgets;
using Mapsui.Utilities;

namespace Mapsui.UI.Forms
{
    /// <summary>
    /// Class, that uses the API of the original Xamarin.Forms MapView
    /// </summary>
    public class MapView : ContentView, IMapControl, INotifyPropertyChanged, IEnumerable<Pin>
    {
        internal MapControl _mapControl;
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

            _mapControl = new MapControl { UseDoubleTap = false };
            MyLocationLayer = new MyLocationLayer(this) { Enabled = true };
            _mapCalloutLayer = new MemoryLayer() { Name = CalloutLayerName, IsMapInfoLayer = true };
            _mapPinLayer = new MemoryLayer() { Name = PinLayerName, IsMapInfoLayer = true };
            _mapDrawableLayer = new MemoryLayer() { Name = DrawableLayerName, IsMapInfoLayer = true };

            // Get defaults from MapControl
            RotationLock = Map.RotationLock;
            ZoomLock = Map.ZoomLock;
            PanLock = Map.PanLock;

            // Add some events to _mapControl
            _mapControl.Viewport.ViewportChanged += HandlerViewportChanged;
            _mapControl.ViewportInitialized += HandlerViewportInitialized;
            _mapControl.Info += HandlerInfo;
            _mapControl.PropertyChanged += HandlerMapControlPropertyChanged;
            _mapControl.SingleTap += HandlerTap;
            _mapControl.DoubleTap += HandlerTap;
            _mapControl.LongTap += HandlerLongTap;
            _mapControl.Hovered += HandlerHovered;
            _mapControl.TouchStarted += HandlerTouchStarted;
            _mapControl.TouchEnded += HandlerTouchEnded;
            _mapControl.TouchEntered += HandlerTouchEntered;
            _mapControl.TouchExited += HandlerTouchExited;
            _mapControl.TouchMove += HandlerTouchMove;
            _mapControl.TouchAction += HandlerTouchAction;
            _mapControl.Swipe += HandlerSwipe;
            _mapControl.Fling += HandlerFling;
            _mapControl.Zoomed += HandlerZoomed;
            _mapControl.SizeChanged += HandlerSizeChanged;

            _mapControl.TouchMove += (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() => MyLocationFollow = false);
            };

            // Add MapView layers to Map
            AddLayers();

            // Add some events to _mapControl.Map.Layers
            _mapControl.Map.Layers.Changed += HandleLayersChanged;

            AbsoluteLayout.SetLayoutBounds(_mapControl, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(_mapControl, AbsoluteLayoutFlags.All);

            _pictMyLocationNoCenter = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.LocationNoCenter.svg", typeof(MapView)));
            _pictMyLocationCenter = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.LocationCenter.svg", typeof(MapView)));

            _pictZoomIn = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.ZoomIn.svg", typeof(MapView)));
            _pictZoomOut = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.ZoomOut.svg", typeof(MapView)));
            _pictNorthing = new SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.RotationZero.svg", typeof(MapView)));

            CreateButtons();

            Content = _mapControl;

            _pins.CollectionChanged += HandlerPinsOnCollectionChanged;
            _drawable.CollectionChanged += HandlerDrawablesOnCollectionChanged;

            _mapCalloutLayer.DataSource = new ObservableCollectionProvider<Callout>(_callouts);
            _mapCalloutLayer.Style = null;  // We don't want a global style for this layer

            _mapPinLayer.DataSource = new ObservableCollectionProvider<Pin>(_pins);
            _mapPinLayer.Style = null;  // We don't want a global style for this layer

            _mapDrawableLayer.DataSource = new ObservableCollectionProvider<Drawable>(_drawable);
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

        /// <summary>
        /// TouchStart is called, when user press a mouse button or touch the display
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchStarted;

        /// <summary>
        /// TouchEnd is called, when user release a mouse button or doesn't touch display anymore
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchEnded;

        /// <summary>
        /// TouchEntered is called, when user moves an active touch onto the view
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchEntered;

        /// <summary>
        /// TouchExited is called, when user moves an active touch off the view
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchExited;

        /// <summary>
        /// TouchMove is called, when user move mouse over map (independent from mouse button state) or move finger on display
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchMove;

        /// <summary>
        /// TouchAction is called, when user provoques a touch event
        /// </summary>
        public event EventHandler<SKTouchEventArgs> TouchAction;

        /// <summary>
        /// Hovered is called, when user move mouse over map without pressing mouse button
        /// </summary>
        public event EventHandler<HoveredEventArgs> Hovered;

        /// <summary>
        /// Swipe is called, when user release mouse button or lift finger while moving with a certain speed 
        /// </summary>
        public event EventHandler<SwipedEventArgs> Swipe;

        /// <summary>
        /// Fling is called, when user release mouse button or lift finger while moving with a certain speed, higher than speed of swipe 
        /// </summary>
        public event EventHandler<SwipedEventArgs> Fling;

        /// <summary>
        /// Zoom is called, when map should be zoomed
        /// </summary>
        public event EventHandler<ZoomedEventArgs> Zoomed;

        /// <inheritdoc />
        public event EventHandler ViewportInitialized;

        /// <inheritdoc />
        public event EventHandler<MapInfoEventArgs> Info;

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

        ///<summary>
        /// Native Mapsui Map object
        ///</summary>
        public Map Map
        {
            get
            {
                return _mapControl.Map;
            }
            set
            {
                if (_mapControl.Map.Equals(value))
                    return;

                if (_mapControl.Map != null)
                {
                    _mapControl.Viewport.ViewportChanged -= HandlerViewportChanged;
                    _mapControl.Info -= HandlerInfo;
                    RemoveButtons();
                    RemoveLayers();
                }

                _mapControl.Map = value;

                CreateButtons();
            }
        }

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
        /// Number of degrees, before the rotation starts
        /// </summary>
        public double UnSnapRotationDegrees
        {
            get { return (double)GetValue(UnSnapRotationDegreesProperty); }
            set { SetValue(UnSnapRotationDegreesProperty, value); }
        }

        /// <summary>
        /// Number of degrees, when map shows to north
        /// </summary>
        public double ReSnapRotationDegrees
        {
            get { return (double)GetValue(ReSnapRotationDegreesProperty); }
            set { SetValue(ReSnapRotationDegreesProperty, value); }
        }

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

        /// <summary>
        /// Enable checks for double tapping. 
        /// But be careful, this will add some extra time, before a single
        /// tap is returned.
        /// </summary>
        public bool UseDoubleTap
        {
            get => (bool)GetValue(UseDoubleTapProperty);
            set => SetValue(UseDoubleTapProperty, value);
        }


        /// <summary>
        /// Enable fling of the map. If touch is lifted during dragging
        /// the map the map will slide a bit more in the same direction.
        /// </summary>
        public bool UseFling
        {
            get => (bool)GetValue(UseFlingProperty);
            set => SetValue(UseFlingProperty, value);
        }

        /// <summary>
        /// Viewport of MapControl
        /// </summary>
        public IReadOnlyViewport Viewport => _mapControl.Viewport;

        /// <summary>
        /// Navigator of MapControl
        /// </summary>
        public INavigator Navigator
        {
            get => _mapControl.Navigator;
            set
            {
                _mapControl.Navigator = value;
            }
        }

        /// <summary>
        /// Underlying MapControl
        /// </summary>
        internal IMapControl MapControl => _mapControl;

        /// <summary>
        /// Update interval for invalidation timer in ms
        /// </summary>
        public int UpdateInterval
        {
            get => _mapControl.UpdateInterval;
            set
            {
                _mapControl.UpdateInterval = value;
            }
        }

        /// <summary>
        /// Object to save performance information about the drawing of the map
        /// </summary>
        /// <remarks>
        /// If this is null, no performance information is saved.
        /// </remarks>
        public Performance Performance
        {
            get => _mapControl.Performance;
            set 
            { 
                _mapControl.Performance = value; 
            }
        }

        /// <summary>
        /// IMapControl
        /// </summary>

        /// <inheritdoc />
        public float PixelDensity => _mapControl.PixelDensity;

        /// <inheritdoc />
        public IRenderer Renderer => _mapControl.Renderer;

        #endregion

        #region IMapControl implementation

        /// <inheritdoc />
        public void Refresh(ChangeType changeType = ChangeType.Discrete)
        {
            _mapControl.Refresh(changeType);
        }

        /// <inheritdoc />
        public MapInfo GetMapInfo(Geometries.Point screenPosition, int margin = 0)
        {
            return MapControl.Renderer.GetMapInfo(screenPosition, Viewport, Map.Layers, margin);
        }

        /// <inheritdoc />
        public byte[] GetSnapshot(IEnumerable<ILayer> layers = null)
        {
            return _mapControl.GetSnapshot(layers);
        }
        
        /// <inheritdoc />
        public void RefreshGraphics()
        {
            _mapControl.RefreshGraphics();
        }

        /// <inheritdoc />
        public void RefreshData(ChangeType changeType = ChangeType.Discrete)
        {
            _mapControl.RefreshData(changeType);
        }

        /// <inheritdoc />
        public void Unsubscribe()
        {
            _mapControl.Unsubscribe();
        }

        /// <inheritdoc />
        public void OpenBrowser(string url)
        {
            _mapControl.OpenBrowser(url);
        }

        /// <inheritdoc />
        public Geometries.Point ToDeviceIndependentUnits(Geometries.Point coordinateInPixels)
        {
            return _mapControl.ToDeviceIndependentUnits(coordinateInPixels);
        }

        /// <inheritdoc />
        public Geometries.Point ToPixels(Geometries.Point coordinateInDeviceIndependentUnits)
        {
            return _mapControl.ToPixels(coordinateInDeviceIndependentUnits);
        }

        #endregion

        /// <summary>
        /// Start updates for control
        /// </summary>
        /// <remarks>
        /// When this function is called, the control is redrawn if needed
        /// </remarks>
        public void StartUpdates(bool refresh = true)
        {
            _mapControl.StartUpdates(refresh);
        }

        /// <summary>
        /// Stop updates for control
        /// </summary>
        /// <remarks>
        /// When this function is called, the control stops to redraw itself, 
        /// even if it is needed
        /// </remarks>
        public void StopUpdates()
        {
            _mapControl.StopUpdates();
        }

        /// <summary>
        /// Force a update of control
        /// </summary>
        /// <remarks>
        /// When this function is called, the control draws itself once 
        /// </remarks>
        public void ForceUpdate()
        {
            _mapControl.ForceUpdate();
        }

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
                    _mapControl.Navigator.CenterOn(MyLocationLayer.MyLocation.ToMapsui());
                }
                else
                {
                    _mapMyLocationButton.Picture = _pictMyLocationNoCenter;
                }

                Refresh();
            }

            if (propertyName.Equals(nameof(UnSnapRotationDegreesProperty)) || propertyName.Equals(nameof(UnSnapRotationDegrees)))
                _mapControl.UnSnapRotationDegrees = UnSnapRotationDegrees;

            if (propertyName.Equals(nameof(ReSnapRotationDegreesProperty)) || propertyName.Equals(nameof(ReSnapRotationDegrees)))
                _mapControl.ReSnapRotationDegrees = ReSnapRotationDegrees;

            if (propertyName.Equals(nameof(RotationLockProperty)) || propertyName.Equals(nameof(RotationLock)))
                _mapControl.Map.RotationLock = RotationLock;

            if (propertyName.Equals(nameof(ZoomLockProperty)) || propertyName.Equals(nameof(ZoomLock)))
                _mapControl.Map.ZoomLock = ZoomLock;

            if (propertyName.Equals(nameof(PanLockProperty)) || propertyName.Equals(nameof(PanLock)))
                _mapControl.Map.PanLock = PanLock;

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

            if (propertyName.Equals(nameof(UseDoubleTapProperty)) || propertyName.Equals(nameof(UseDoubleTap)))
                _mapControl.UseDoubleTap = UseDoubleTap;

            if (propertyName.Equals(nameof(UseFlingProperty)) || propertyName.Equals(nameof(UseFlingProperty)))
                _mapControl.UseFling = UseFling;
        }

        #region Handlers

        /// <summary>
        /// MapControl has changed
        /// </summary>
        /// <param name="sender">MapControl of this event</param>
        /// <param name="e">Event arguments containing what changed</param>
        private void HandlerMapControlPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(MapControl.Map)))
            {
                if (_mapControl.Map != null)
                {
                    // Remove MapView layers
                    RemoveLayers();

                    // Readd them, so that they always on top
                    AddLayers();

                    // Add event handlers
                    _mapControl.Viewport.ViewportChanged += HandlerViewportChanged;
                    _mapControl.Info += HandlerInfo;
                }
            }
        }

        /// <summary>
        /// Viewport of map has changed
        /// </summary>
        /// <param name="sender">Viewport of this event</param>
        /// <param name="e">Event arguments containing what changed</param>
        private void HandlerViewportChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Viewport.Rotation)))
            {
                MyLocationLayer.UpdateMyDirection(MyLocationLayer.Direction, _mapControl.Viewport.Rotation);

                // Update rotationButton
                _mapNorthingButton.Rotation = (float)_mapControl.Viewport.Rotation;
            }

            if (e.PropertyName.Equals(nameof(Viewport.Center)))
            {
                if (MyLocationFollow && !_mapControl.Viewport.Center.Equals(MyLocationLayer.MyLocation.ToMapsui()))
                {
                    //_mapControl.Map.NavigateTo(_mapMyLocationLayer.MyLocation.ToMapsui());
                }
            }
        }

        private void HandlerViewportInitialized(object sender, EventArgs e)
        {
            ViewportInitialized?.Invoke(sender, e);
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

        private void HandlerHovered(object sender, HoveredEventArgs e)
        {
            Hovered?.Invoke(sender, e);
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

                    var pinArgs = new PinClickedEventArgs(clickedPin, _mapControl.Viewport.ScreenToWorld(e.MapInfo.ScreenPosition).ToForms(), e.NumTaps);

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
                    _mapControl.Viewport.ScreenToWorld(e.MapInfo.ScreenPosition).ToForms(),
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
                    _mapControl.Viewport.ScreenToWorld(e.MapInfo.ScreenPosition).ToForms(),
                    new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.NumTaps);

                clickedDrawable?.HandleClicked(drawableArgs);

                e.Handled = drawableArgs.Handled;
                
                return;
            }

            // Call Info event, if there is one
            Info?.Invoke(sender, e);
        }

        private void HandlerLongTap(object sender, TappedEventArgs e)
        {
            var args = new MapLongClickedEventArgs(_mapControl.Viewport.ScreenToWorld(e.ScreenPosition).ToForms());

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
                foreach (var widget in _mapControl.Map.Widgets)
                {
                    if (widget.Enabled && widget.Envelope.Contains(e.ScreenPosition))
                    {
                        if (widget.HandleWidgetTouched(_mapControl.Navigator, e.ScreenPosition))
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }

                // Check, if we hit a drawable
                // Is there a drawable at this position
                var mapInfo = _mapControl.GetMapInfo(e.ScreenPosition);

                if (mapInfo.Feature == null)
                {
                    var args = new MapClickedEventArgs(_mapControl.Viewport.ScreenToWorld(e.ScreenPosition).ToForms(), e.NumOfTaps);

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

        private void HandlerZoomed(object sender, ZoomedEventArgs e)
        {
            Zoomed?.Invoke(sender, e);
        }

        private void HandlerFling(object sender, SwipedEventArgs e)
        {
            Fling?.Invoke(sender, e);
        }

        private void HandlerSwipe(object sender, SwipedEventArgs e)
        {
            Swipe?.Invoke(sender, e);
        }

        private void HandlerTouchEnded(object sender, TouchedEventArgs e)
        {
            TouchEnded?.Invoke(sender, e);
        }

        private void HandlerTouchMove(object sender, TouchedEventArgs e)
        {
            TouchMove?.Invoke(sender, e);
        }

        private void HandlerTouchStarted(object sender, TouchedEventArgs e)
        {
            TouchStarted?.Invoke(sender, e);
        }
        private void HandlerTouchEntered(object sender, TouchedEventArgs e)
        {
            TouchEntered?.Invoke(sender, e);
        }
        private void HandlerTouchExited(object sender, TouchedEventArgs e)
        {
            TouchExited?.Invoke(sender, e);
        }

        private void HandlerTouchAction(object sender, SKTouchEventArgs e)
        {
            TouchAction?.Invoke(sender, e);
        }

        private void HandlerPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.RefreshData(_mapControl.Viewport.Extent, _mapControl.Viewport.Resolution, ChangeType.Continuous);

            // Repaint map, because something could have changed
            _mapControl.RefreshGraphics();
        }

        private void HandlerDrawablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.RefreshData(_mapControl.Viewport.Extent, _mapControl.Viewport.Resolution, ChangeType.Continuous);

            // Repaint map, because something could have changed
            _mapControl.RefreshGraphics();
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
            // Add MapView layers
            _mapControl.Map.Layers.Add(_mapDrawableLayer, _mapPinLayer, _mapCalloutLayer, MyLocationLayer);
        }

        /// <summary>
        /// Remove all layers that MapView uses
        /// </summary>
        private void RemoveLayers()
        {
            // Remove MapView layers
            _mapControl.Map.Layers.Remove(MyLocationLayer, _mapCalloutLayer, _mapPinLayer, _mapDrawableLayer);
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
            if (layer.MinVisible > _mapControl.Viewport.Resolution) return drawables;
            if (layer.MaxVisible < _mapControl.Viewport.Resolution) return drawables;

            var allFeatures = layer.GetFeaturesInView(layer.Envelope, _mapControl.Viewport.Resolution);
            var mapInfo = _mapControl.GetMapInfo(point);

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

            // If there more than one drawables found, than reverse, because the top most should be the first
            if (drawables.Count > 1)
                drawables.Reverse();

            return drawables;
        }

        private void UpdateButtonPositions()
        {
            var newX = _mapControl.Width - ButtonMargin.Right - ButtonSize;
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

            _mapControl.RefreshGraphics();
        }

        private void RemoveButtons()
        {
            var widgets = _mapControl.Map.Widgets.ToList();
            widgets.Remove(_mapZoomInButton);
            widgets.Remove(_mapZoomOutButton); 
            widgets.Remove(_mapMyLocationButton);
            widgets.Remove(_mapNorthingButton);
            _mapControl.Map.Widgets.Clear();
            _mapControl.Map.Widgets.AddRange(widgets);

            _mapControl.RefreshGraphics();
        }

        private void CreateButtons()
        {
            _mapZoomInButton = CreateButton(0, 0, _pictZoomIn, (s, e) => { _mapControl.Navigator.ZoomIn(); e.Handled = true; });
            _mapZoomInButton.Enabled = IsZoomButtonVisible;
            _mapControl.Map.Widgets.Add(_mapZoomInButton);

            _mapZoomOutButton = CreateButton(0, 40, _pictZoomOut, (s, e) => { _mapControl.Navigator.ZoomOut(); e.Handled = true; });
            _mapZoomOutButton.Enabled = IsZoomButtonVisible;
            _mapControl.Map.Widgets.Add(_mapZoomOutButton);

            _mapMyLocationButton = CreateButton(0, 88, _pictMyLocationNoCenter, (s, e) => { MyLocationFollow = true; e.Handled = true; });
            _mapMyLocationButton.Enabled = IsMyLocationButtonVisible;
            _mapControl.Map.Widgets.Add(_mapMyLocationButton);

            _mapNorthingButton = CreateButton(0, 136, _pictNorthing, (s, e) => { Device.BeginInvokeOnMainThread(() => _mapControl.Navigator.RotateTo(0)); e.Handled = true; });
            _mapNorthingButton.Enabled = IsNorthingButtonVisible;
            _mapControl.Map.Widgets.Add(_mapNorthingButton);

            UpdateButtonPositions();
        }

        private ButtonWidget CreateButton(float x, float y, SKPicture picture, Action<object, WidgetTouchedEventArgs> action)
        {
            var result = new Widgets.Button.ButtonWidget
            {
                Picture = picture,
                Envelope = new Geometries.BoundingBox(x, y, x + ButtonSize, y + ButtonSize),
                Rotation = 0,
                Enabled = true,
            };
            result.WidgetTouched += (s, e) => action(s, e);
            result.PropertyChanged += (s, e) => _mapControl.RefreshGraphics();

            return result;
        }
    }
}
