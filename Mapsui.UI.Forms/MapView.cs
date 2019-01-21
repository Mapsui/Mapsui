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
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    /// <summary>
    /// Class, that uses the API of the original Xamarin.Forms MapView
    /// </summary>
    public class MapView : ContentView, IMapControl, INotifyPropertyChanged, IEnumerable<Pin>
    {
        internal MapControl _mapControl;

        private readonly MyLocationLayer _mapMyLocationLayer;
        private const string PinLayerName = "Pins";
        private const string DrawableLayerName = "Drawables";
        private readonly Layer _mapPinLayer;
        private readonly Layer _mapDrawableLayer;
        private readonly StackLayout _mapButtons;
        private readonly SvgButton _mapZoomInButton;
        private readonly SvgButton _mapZoomOutButton;
        private readonly Image _mapSpacingButton1;
        private readonly SvgButton _mapMyLocationButton;
        private readonly Image _mapSpacingButton2;
        private readonly SvgButton _mapNorthingButton;
        private readonly SKPicture _pictMyLocationNoCenter;
        private readonly SKPicture _pictMyLocationCenter;

        readonly ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
        readonly ObservableCollection<Drawable> _drawable = new ObservableCollection<Drawable>();
        readonly ObservableCollection<Callout> _callouts = new ObservableCollection<Callout>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.MapView"/> class.
        /// </summary>
        public MapView()
        {
            MyLocationEnabled = false;
            MyLocationFollow = false;

            IsClippedToBounds = true;

            _mapControl = new MapControl { UseDoubleTap = false };
            _mapMyLocationLayer = new MyLocationLayer(this) { Enabled = true };
            _mapPinLayer = new Layer(PinLayerName) { IsMapInfoLayer = true };
            _mapDrawableLayer = new Layer(DrawableLayerName) { IsMapInfoLayer = true };

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
            _mapControl.Hovered += HandlerHover;
            _mapControl.TouchMove += (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() => MyLocationFollow = false);
            };

            AbsoluteLayout.SetLayoutBounds(_mapControl, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(_mapControl, AbsoluteLayoutFlags.All);

            _pictMyLocationNoCenter = new SkiaSharp.Extended.Svg.SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.LocationNoCenter.svg", typeof(MapView)));
            _pictMyLocationCenter = new SkiaSharp.Extended.Svg.SKSvg().Load(Utilities.EmbeddedResourceLoader.Load("Images.LocationCenter.svg", typeof(MapView)));

            _mapZoomInButton = new SvgButton(Utilities.EmbeddedResourceLoader.Load("Images.ZoomIn.svg", typeof(MapView)))
            {
                BackgroundColor = Color.White,
                WidthRequest = 40,
                HeightRequest = 40,
                Command = new Command(obj => { _mapControl.Navigator.ZoomIn(); Refresh(); })
            };

            _mapZoomOutButton = new SvgButton(Utilities.EmbeddedResourceLoader.Load("Images.ZoomOut.svg", typeof(MapView)))
            {
                BackgroundColor = Color.White,
                WidthRequest = 40,
                HeightRequest = 40,
                Command = new Command(obj => { _mapControl.Navigator.ZoomOut(); Refresh(); }),
            };

            _mapSpacingButton1 = new Image { BackgroundColor = Color.Transparent, WidthRequest = 40, HeightRequest = 8 };

            _mapMyLocationButton = new SvgButton(_pictMyLocationNoCenter)
            {
                BackgroundColor = Color.White,
                WidthRequest = 40,
                HeightRequest = 40,
                Command = new Command(obj => MyLocationFollow = !MyLocationFollow),
            };

            _mapSpacingButton2 = new Image { BackgroundColor = Color.Transparent, WidthRequest = 40, HeightRequest = 8 };

            _mapNorthingButton = new SvgButton(Utilities.EmbeddedResourceLoader.Load("Images.RotationZero.svg", typeof(MapView)))
            {
                BackgroundColor = Color.White,
                WidthRequest = 40,
                HeightRequest = 40,
                Command = new Command(obj => Device.BeginInvokeOnMainThread(() => _mapControl.Navigator.RotateTo(0))),
            };

            _mapButtons = new StackLayout { BackgroundColor = Color.Transparent, Opacity = 0.8, Spacing = 0, IsVisible = true };

            _mapButtons.Children.Add(_mapZoomInButton);
            _mapButtons.Children.Add(_mapZoomOutButton);
            _mapButtons.Children.Add(_mapSpacingButton1);
            _mapButtons.Children.Add(_mapMyLocationButton);
            _mapButtons.Children.Add(_mapSpacingButton2);
            _mapButtons.Children.Add(_mapNorthingButton);

            AbsoluteLayout.SetLayoutBounds(_mapButtons, new Rectangle(0.95, 0.03, 40, 176));
            AbsoluteLayout.SetLayoutFlags(_mapButtons, AbsoluteLayoutFlags.PositionProportional);

            Content = new AbsoluteLayout
            {
                Children = {
                    _mapControl,
                    _mapButtons,
                }
            };

            _pins.CollectionChanged += HandlerPinsOnCollectionChanged;
            _drawable.CollectionChanged += HandlerDrawablesOnCollectionChanged;

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

        /// <inheritdoc />
        public event EventHandler ViewportInitialized;

        /// <inheritdoc />
        public event EventHandler<MapInfoEventArgs> Info;

        #endregion

        #region Bindings

        public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(nameof(SelectedPin), typeof(Pin), typeof(MapView), default(Pin), defaultBindingMode: BindingMode.TwoWay);
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
        public static readonly BindableProperty UseDoubleTapProperty = BindableProperty.Create(nameof(UseDoubleTapProperty), typeof(bool), typeof(MapView), default(bool));

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
                    _mapControl.Map.Layers.Remove(_mapPinLayer);
                    _mapControl.Map.Layers.Remove(_mapDrawableLayer);
                    _mapControl.Map.Layers.Remove(_mapMyLocationLayer);
                }

                _mapControl.Map = value;
            }
        }

        /// <summary>
        /// MyLocation layer
        /// </summary>
        public MyLocationLayer MyLocationLayer => _mapMyLocationLayer;

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
        /// Enable checks for double tapping. 
        /// But be carefull, this will add some extra time, before a single
        /// tap is returned.
        /// </summary>
        public bool UseDoubleTap
        {
            get => (bool)GetValue(UseDoubleTapProperty);
            set => SetValue(UseDoubleTapProperty, value);
        }

        /// <summary>
        /// Viewport of MapControl
        /// </summary>
        public IReadOnlyViewport Viewport => _mapControl.Viewport;

        /// <summary>
        /// Navigator of MapControl
        /// </summary>
        public INavigator Navigator => _mapControl.Navigator;

        /// <summary>
        /// Underlying MapControl
        /// </summary>
        internal IMapControl MapControl => _mapControl;

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
        public void Refresh()
        {
            _mapControl.Refresh();
        }

        /// <inheritdoc />
        public MapInfo GetMapInfo(Geometries.Point screenPosition, int margin = 0)
        {
            return MapInfoHelper.GetMapInfo(Map.Layers.Where(l => l.IsMapInfoLayer), Viewport,
                screenPosition, _mapControl.Renderer.SymbolCache, margin);
        }

        /// <inheritdoc />
        public MapInfo GetMapInfo(IEnumerable<ILayer> layers, Geometries.Point screenPosition, int margin = 0)
        {
            return MapInfoHelper.GetMapInfo(layers, Viewport,
                screenPosition, _mapControl.Renderer.SymbolCache, margin);
        }

        /// <inheritdoc />
        public void RefreshGraphics()
        {
            _mapControl.RefreshGraphics();
        }

        /// <inheritdoc />
        public void RefreshData()
        {
            _mapControl.RefreshData();
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

        #region Callouts

        private Callout _callout;

        /// <summary>
        /// Creates a callout at the given position
        /// </summary>
        /// <returns>The callout</returns>
        /// <param name="position">Position of callout</param>
        public Callout CreateCallout(Position position)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _callout = new Callout(_mapControl)
                {
                    Anchor = position
                };
            });

            // My interpretation (PDD): This while keeps looping until the asynchronous call
            // above has created a callout.
            // An alternative might be to avoid CreateCallout from a non-ui thread by throwing
            // early.
            while (_callout == null) ;

            var result = _callout;
            _callout = null;

            return result;
        }

        /// <summary>
        /// Shows given callout
        /// </summary>
        /// <param name="callout">Callout to show</param>
        public void ShowCallout(Callout callout)
        {
            if (callout == null)
                return;

            // Set absolute layout constrains
            AbsoluteLayout.SetLayoutFlags(callout, AbsoluteLayoutFlags.None);

            // Add it to MapView
            if (!((AbsoluteLayout)Content).Children.Contains(callout))
                Device.BeginInvokeOnMainThread(() => ((AbsoluteLayout)Content).Children.Add(callout));

            // Add it to list of active Callouts
            _callouts.Add(callout);

            // When Callout is closed by close button
            callout.CalloutClosed += (s, e) => HideCallout((Callout)s);

            // Inform Callout
            callout.Show();
        }

        /// <summary>
        /// Hides given callout
        /// </summary>
        /// <param name="callout">Callout to hide</param>
        public void HideCallout(Callout callout)
        {
            if (callout == null)
                return;

            // Inform Callout
            callout.Hide();

            // Remove it from list of active Callouts
            _callouts.Remove(callout);

            // Remove it from MapView
            if (((AbsoluteLayout)Content).Children.Contains(callout))
                Device.BeginInvokeOnMainThread(() => ((AbsoluteLayout)Content).Children.Remove(callout));
        }

        #endregion

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
                _mapMyLocationLayer.Enabled = MyLocationEnabled;
                Refresh();
            }

            if (propertyName.Equals(nameof(MyLocationFollowProperty)) || propertyName.Equals(nameof(MyLocationFollow)))
            {
                _mapMyLocationButton.IsEnabled = !MyLocationFollow;

                if (MyLocationFollow)
                {
                    _mapMyLocationButton.Picture = _pictMyLocationCenter;
                    _mapControl.Navigator.CenterOn(_mapMyLocationLayer.MyLocation.ToMapsui());
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
                _mapZoomInButton.IsVisible = IsZoomButtonVisible;
                _mapZoomOutButton.IsVisible = IsZoomButtonVisible;
                _mapSpacingButton1.IsVisible = IsZoomButtonVisible && IsMyLocationButtonVisible;
            }

            if (propertyName.Equals(nameof(IsMyLocationButtonVisibleProperty)) || propertyName.Equals(nameof(IsMyLocationButtonVisible)))
            {
                _mapMyLocationButton.IsVisible = IsMyLocationButtonVisible;
                _mapSpacingButton1.IsVisible = IsZoomButtonVisible && IsMyLocationButtonVisible;
            }

            if (propertyName.Equals(nameof(IsNorthingButtonVisibleProperty)) || propertyName.Equals(nameof(IsNorthingButtonVisible)))
            {
                _mapNorthingButton.IsVisible = IsNorthingButtonVisible;
                _mapSpacingButton2.IsVisible = (IsMyLocationButtonVisible || IsZoomButtonVisible) && IsNorthingButtonVisible;
            }

            if (propertyName.Equals(nameof(UseDoubleTapProperty)) || propertyName.Equals(nameof(UseDoubleTap)))
                _mapControl.UseDoubleTap = UseDoubleTap;
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
                    // Add layer for MyLocation
                    if (!_mapControl.Map.Layers.Contains(_mapMyLocationLayer))
                        _mapControl.Map.Layers.Add(_mapMyLocationLayer);
                    // Draw drawables first
                    if (!_mapControl.Map.Layers.Contains(_mapDrawableLayer))
                        _mapControl.Map.Layers.Add(_mapDrawableLayer);
                    // Draw pins on top of drawables
                    if (!_mapControl.Map.Layers.Contains(_mapPinLayer))
                        _mapControl.Map.Layers.Add(_mapPinLayer);
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
                _mapMyLocationLayer.UpdateMyDirection(_mapMyLocationLayer.Direction, _mapControl.Viewport.Rotation);

                // Check all callout positions
                var list = _callouts.ToList();

                // First check all Callouts, that belong to a pin
                foreach (var pin in _pins)
                {
                    if (pin.Callout != null)
                    {
                        pin.UpdateCalloutPosition();
                        list.Remove(pin.Callout);
                    }
                }

                // Now check the rest, Callouts not belonging to a pin
                foreach (var c in list)
                    c.UpdateScreenPosition();

            }
            if (e.PropertyName.Equals(nameof(Viewport.Center)))
            {
                if (MyLocationFollow && !_mapControl.Viewport.Center.Equals(_mapMyLocationLayer.MyLocation.ToMapsui()))
                {
                    //_mapControl.Map.NavigateTo(_mapMyLocationLayer.MyLocation.ToMapsui());
                }
            }
        }

        private void HandlerViewportInitialized(object sender, EventArgs e)
        {
            ViewportInitialized?.Invoke(sender, e);
        }

        private void HandlerHover(object sender, HoveredEventArgs e)
        {
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
                        HideCallout(pin.Callout);
                        pin.Callout = null;

                        pin.PropertyChanged -= HandlerPinPropertyChanged;

                        if (SelectedPin.Equals(pin))
                            SelectedPin = null;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    // Add new pins to layer
                    var pin = item as Pin;

                    if (pin != null) pin.PropertyChanged += HandlerPinPropertyChanged;
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

            foreach (var item in e.NewItems)
            {
                // Add new drawables to layer
                if (item is INotifyPropertyChanged drawable)
                    drawable.PropertyChanged += HandlerDrawablePropertyChanged;
            }

            Refresh();
        }

        private void HandlerInfo(object sender, MapInfoEventArgs e)
        {
            // Click on pin?
            Pin clickedPin = null;

            foreach (var pin in _pins)
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

            // Check for clicked drawables
            var drawables = GetDrawablesAt(_mapControl.Viewport.ScreenToWorld(e.MapInfo.ScreenPosition), _mapDrawableLayer);

            var drawableArgs = new DrawableClickedEventArgs(
                _mapControl.Viewport.ScreenToWorld(e.MapInfo.ScreenPosition).ToForms(), 
                new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.NumTaps);

            // Now check each drawable until one handles the event
            foreach (var drawable in drawables)
            {
                drawable.HandleClicked(drawableArgs);

                if (!drawableArgs.Handled) continue;
                e.Handled = true;
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
            var list = _callouts.ToList();

            // First check all Callouts, that belong to a pin
            foreach (var pin in _pins)
            {
                if (pin.Callout != null)
                {
                    if (pin.Callout.IsClosableByClick)
                        pin.IsCalloutVisible = false;
                    list.Remove(pin.Callout);
                }
            }

            // Now check the rest, Callouts not belonging to a pin
            foreach (var c in list)
                if (c.IsClosableByClick)
                    HideCallout(c);

            e.Handled = false;

            // Check, if we hit a widget or drawable
            // Is there a widget at this position
            // Is there a drawable at this position
            if (Map != null)
            {
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
                HandlerInfo(sender, new MapInfoEventArgs { MapInfo = mapInfo, Handled = e.Handled, NumTaps = e.NumOfTaps });
            }
        }

        private void HandlerPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.RefreshData(_mapControl.Viewport.Extent, _mapControl.Viewport.Resolution, false);
        }

        private void HandlerDrawablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.RefreshData(_mapControl.Viewport.Extent, _mapControl.Viewport.Resolution, false);
        }

        #endregion

        /// <summary>
        /// Get all drawables of layer that contain given point
        /// </summary>
        /// <param name="point">Point to search for in world coordinates</param>
        /// <param name="layer">Layer to search for drawables</param>
        /// <returns>List with all drawables at point, which are clickable</returns>
        private IList<Drawable> GetDrawablesAt(Geometries.Point point, Layer layer)
        {
            List<Drawable> drawables = new List<Drawable>();

            if (layer.Enabled == false) return drawables;
            if (layer.MinVisible > _mapControl.Viewport.Resolution) return drawables;
            if (layer.MaxVisible < _mapControl.Viewport.Resolution) return drawables;

            var allFeatures = layer.GetFeaturesInView(layer.Envelope, _mapControl.Viewport.Resolution);

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
    }
}