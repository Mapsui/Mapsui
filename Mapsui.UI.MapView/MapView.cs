using Mapsui.Layers;
using Mapsui.UI.Objects;
using Mapsui.Widgets;
using Mapsui.Extensions;
using Mapsui.Widgets.ButtonWidget;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using Mapsui.Logging;
using Mapsui.Utilities;
#if __MAUI__
using Mapsui.UI.Maui.Utils;
using Mapsui.UI.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using SkiaSharp.Views;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Rectangle = Microsoft.Maui.Graphics.Rect;
#else
using Mapsui.UI.Forms.Utils;
using Mapsui.UI.Forms.Extensions;
using Xamarin.Forms;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

/// <summary>
/// Class, that uses the API of the original Xamarin.Forms MapView
/// </summary>
public class MapView : MapControl, INotifyPropertyChanged, IEnumerable<Pin>
{
    private const string CalloutLayerName = "Callouts";
    private const string PinLayerName = "Pins";
    private const string DrawableLayerName = "Drawables";
    private readonly ObservableMemoryLayer<Callout> _mapCalloutLayer;
    private readonly ObservableMemoryLayer<Pin> _mapPinLayer;
    private readonly ObservableMemoryLayer<Drawable> _mapDrawableLayer;
    private ButtonWidget? _mapZoomInButton;
    private ButtonWidget? _mapZoomOutButton;
    private ButtonWidget? _mapMyLocationButton;
    private ButtonWidget? _mapNorthingButton;
    private readonly SKPicture _pictMyLocationNoCenter;
    private readonly SKPicture _pictMyLocationCenter;
    private readonly SKPicture _pictZoomIn;
    private readonly SKPicture _pictZoomOut;
    private readonly SKPicture _pictNorthing;
    private readonly ObservableRangeCollection<Pin> _pins = new ObservableRangeCollection<Pin>();
    private readonly ObservableRangeCollection<Drawable> _drawable = new ObservableRangeCollection<Drawable>();
    private readonly ObservableRangeCollection<Callout> _callouts = new ObservableRangeCollection<Callout>();

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.MapView"/> class.
    /// </summary>
    public MapView()
    {
        MyLocationFollow = false;

        IsClippedToBounds = true;
        UseDoubleTap = false;

        MyLocationLayer = new MyLocationLayer(this) { Enabled = true };
        _mapCalloutLayer = new ObservableMemoryLayer<Callout>(f => f.Feature) { Name = CalloutLayerName, IsMapInfoLayer = true };
        _mapPinLayer = new ObservableMemoryLayer<Pin>(f => f.Feature) { Name = PinLayerName, IsMapInfoLayer = true };
        _mapDrawableLayer = new ObservableMemoryLayer<Drawable>(f => f.Feature) { Name = DrawableLayerName, IsMapInfoLayer = true };

        // Get defaults from MapControl
        RotationLock = Map.Navigator.RotationLock;
        ZoomLock = Map.Navigator.ZoomLock;
        PanLock = Map.Navigator.PanLock;

        // Add some events to _mapControl
        Map.Navigator.ViewportChanged += HandlerViewportChanged;
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

#pragma warning disable IDISP004 // Don't ignore created IDisposable
        _pictMyLocationNoCenter = EmbeddedResourceLoader.Load("Images.LocationNoCenter.svg", typeof(MapView)).LoadSvgPicture() ?? throw new MissingManifestResourceException("Images.LocationNoCenter.svg");
        _pictMyLocationCenter = EmbeddedResourceLoader.Load("Images.LocationCenter.svg", typeof(MapView)).LoadSvgPicture() ?? throw new MissingManifestResourceException("Images.LocationCenter.svg");

        _pictZoomIn = EmbeddedResourceLoader.Load("Images.ZoomIn.svg", typeof(MapView)).LoadSvgPicture() ?? throw new MissingManifestResourceException("Images.ZoomIn.svg");
        _pictZoomOut = EmbeddedResourceLoader.Load("Images.ZoomOut.svg", typeof(MapView)).LoadSvgPicture() ?? throw new MissingManifestResourceException("Images.ZoomOut.svg");
        _pictNorthing = EmbeddedResourceLoader.Load("Images.RotationZero.svg", typeof(MapView)).LoadSvgPicture() ?? throw new MissingManifestResourceException("Images.RotationZero.svg");
#pragma warning restore IDISP001
        CreateButtons();

        _pins.CollectionChanged += HandlerPinsOnCollectionChanged;
        _drawable.CollectionChanged += HandlerDrawablesOnCollectionChanged;

        _mapCalloutLayer.ObservableCollection = _callouts;
        _mapCalloutLayer.Style = null;  // We don't want a global style for this layer

        _mapPinLayer.ObservableCollection = _pins;
        _mapPinLayer.Style = null;  // We don't want a global style for this layer

        _mapDrawableLayer.ObservableCollection = _drawable;
        _mapDrawableLayer.Style = null;  // We don't want a global style for this layer
    }

    #region Events

    ///<summary>
    /// Occurs when a pin clicked
    /// </summary>
    public event EventHandler<PinClickedEventArgs>? PinClicked;

    /// <summary>
    /// Occurs when selected pin changed
    /// </summary>
    public event EventHandler<SelectedPinChangedEventArgs>? SelectedPinChanged;

    /// <summary>
    /// Occurs when map clicked
    /// </summary>
    public event EventHandler<MapClickedEventArgs>? MapClicked;

    /// <summary>
    /// Occurs when map long clicked
    /// </summary>
    public event EventHandler<MapLongClickedEventArgs>? MapLongClicked;

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
        get => (bool)GetValue(MyLocationEnabledProperty);
#if __MAUI__ // WORKAROUND for Preview 11 will be fixed in Preview 13 https://github.com/dotnet/maui/issues/3597
        set => Application.Current?.Dispatcher.Dispatch(() =>
        {
            try
            {
                SetValue(MyLocationEnabledProperty, value);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        });
#else
        set => Device.BeginInvokeOnMainThread(() =>
        {
            try
            {
                SetValue(MyLocationEnabledProperty, value);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        });
#endif
    }

    /// <summary>
    /// Should center of map follow my location
    /// </summary>
    public bool MyLocationFollow
    {
        get => (bool)GetValue(MyLocationFollowProperty);
        set => SetValue(MyLocationFollowProperty, value);
    }

    /// <summary>
    /// Pins on map
    /// </summary>
    public IList<Pin> Pins => _pins;

    /// <summary>
    /// Selected pin
    /// </summary>
    public Pin? SelectedPin
    {
        get => (Pin?)GetValue(SelectedPinProperty);
        set => SetValue(SelectedPinProperty, value);
    }

    /// <summary>
    /// Single or multiple callouts possible
    /// </summary>
    public bool UniqueCallout
    {
        get => (bool)GetValue(UniqueCalloutProperty);
        set => SetValue(UniqueCalloutProperty, value);
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
        get => (bool)GetValue(RotationLockProperty);
        set => SetValue(RotationLockProperty, value);
    }

    /// <summary>
    /// Enable zooming
    /// </summary>
    public bool ZoomLock
    {
        get => (bool)GetValue(ZoomLockProperty);
        set => SetValue(ZoomLockProperty, value);
    }

    /// <summary>
    /// Enable paning
    /// </summary>
    public bool PanLock
    {
        get => (bool)GetValue(PanLockProperty);
        set => SetValue(PanLockProperty, value);
    }

    /// <summary>
    /// Enable zoom buttons
    /// </summary>
    public bool IsZoomButtonVisible
    {
        get => (bool)GetValue(IsZoomButtonVisibleProperty);
        set => SetValue(IsZoomButtonVisibleProperty, value);
    }

    /// <summary>
    /// Enable My Location button
    /// </summary>
    public bool IsMyLocationButtonVisible
    {
        get => (bool)GetValue(IsMyLocationButtonVisibleProperty);
        set => SetValue(IsMyLocationButtonVisibleProperty, value);
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

    internal void RemoveCallout(Callout? callout)
    {
        if (callout != null && _callouts.Contains(callout))
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
                _mapMyLocationButton!.Picture = _pictMyLocationCenter;
                Map.Navigator.CenterOn(MyLocationLayer.MyLocation.ToMapsui());
            }
            else
            {
                _mapMyLocationButton!.Picture = _pictMyLocationNoCenter;
            }

            Refresh();
        }

        if (Map != null && (propertyName.Equals(nameof(RotationLockProperty)) || propertyName.Equals(nameof(RotationLock))))
            Map.Navigator.RotationLock = RotationLock;

        if (Map != null && (propertyName.Equals(nameof(ZoomLockProperty)) || propertyName.Equals(nameof(ZoomLock))))
            Map.Navigator.ZoomLock = ZoomLock;

        if (Map != null && (propertyName.Equals(nameof(PanLockProperty)) || propertyName.Equals(nameof(PanLock))))
            Map.Navigator.PanLock = PanLock;

        if (propertyName.Equals(nameof(IsZoomButtonVisibleProperty)) || propertyName.Equals(nameof(IsZoomButtonVisible)))
        {
            _mapZoomInButton!.Enabled = IsZoomButtonVisible;
            _mapZoomOutButton!.Enabled = IsZoomButtonVisible;
            UpdateButtonPositions();
        }

        if (propertyName.Equals(nameof(IsMyLocationButtonVisibleProperty)) || propertyName.Equals(nameof(IsMyLocationButtonVisible)))
        {
            _mapMyLocationButton!.Enabled = IsMyLocationButtonVisible;
            UpdateButtonPositions();
        }

        if (propertyName.Equals(nameof(IsNorthingButtonVisibleProperty)) || propertyName.Equals(nameof(IsNorthingButtonVisible)))
        {
            _mapNorthingButton!.Enabled = IsNorthingButtonVisible;
            UpdateButtonPositions();
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
                Map.Navigator.ViewportChanged += HandlerViewportChanged;
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
    private void HandlerViewportChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.Equals(nameof(Navigator.Viewport.Rotation)) ?? false)
        {
            MyLocationLayer.UpdateMyDirection(MyLocationLayer.Direction, Map.Navigator.Viewport.Rotation);

            // Update rotationButton
            _mapNorthingButton!.Rotation = (float)Map.Navigator.Viewport.Rotation;
        }
    }

    private void HandleLayersChanged(object? sender, LayerCollectionChangedEventArgs args)
    {
        var localRemovedLayers = args.RemovedLayers?.ToList() ?? new List<ILayer>();
        var localAddedLayers = args.AddedLayers?.ToList() ?? new List<ILayer>();
        var movedLayers = args.MovedLayers?.ToList() ?? new List<ILayer>();

        if (localRemovedLayers.Contains(MyLocationLayer) || localRemovedLayers.Contains(_mapDrawableLayer) || localRemovedLayers.Contains(_mapPinLayer) || localRemovedLayers.Contains(_mapCalloutLayer) ||
            localAddedLayers.Contains(MyLocationLayer) || localAddedLayers.Contains(_mapDrawableLayer) || localAddedLayers.Contains(_mapPinLayer) || localAddedLayers.Contains(_mapCalloutLayer) ||
            movedLayers.Contains(MyLocationLayer) || movedLayers.Contains(_mapDrawableLayer) || movedLayers.Contains(_mapPinLayer) || movedLayers.Contains(_mapCalloutLayer))
            return;

        // Remove MapView layers
        RemoveLayers();

        // Readd them, so that they always on top
        AddLayers();
    }

    private void HandlerPinsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

    private void HandlerDrawablesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

    private void HandlerInfo(object? sender, MapInfoEventArgs e)
    {
        // Click on pin?
        if (e.MapInfo?.Layer == _mapPinLayer)
        {
            Pin? clickedPin = null;
            var pins = _pins.ToList();

            foreach (var pin in pins)
            {
                if (pin.IsVisible && (pin.Feature?.Equals(e.MapInfo.Feature) ?? false))
                {
                    clickedPin = pin;
                    break;
                }
            }

            if (clickedPin != null)
            {
                SelectedPin = clickedPin;

                SelectedPinChanged?.Invoke(this, new SelectedPinChangedEventArgs(SelectedPin));

                if (e.MapInfo!.ScreenPosition == null)
                    return;

                var pinArgs = new PinClickedEventArgs(clickedPin, Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(), e.NumTaps);

                PinClicked?.Invoke(this, pinArgs);

                if (pinArgs.Handled)
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        // Check for clicked callouts
        else if (e.MapInfo?.Layer == _mapCalloutLayer)
        {
            Callout? clickedCallout = null;
            var callouts = _callouts.ToList();

            foreach (var callout in callouts)
            {
                if (callout.Feature.Equals(e.MapInfo.Feature))
                {
                    clickedCallout = callout;
                    break;
                }
            }

            if (e.MapInfo!.ScreenPosition == null)
                return;

            var calloutArgs = new CalloutClickedEventArgs(clickedCallout,
                Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(),
                new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.NumTaps);

            clickedCallout?.HandleCalloutClicked(this, calloutArgs);

            e.Handled = calloutArgs.Handled;

            return;
        }
        // Check for clicked drawables
        else if (e.MapInfo?.Layer == _mapDrawableLayer)
        {
            Drawable? clickedDrawable = null;
            var drawables = _drawable.ToList();

            foreach (var drawable in drawables)
            {
                if (drawable.IsClickable && (drawable.Feature?.Equals(e.MapInfo.Feature) ?? false))
                {
                    clickedDrawable = drawable;
                    break;
                }
            }

            if (e.MapInfo!.ScreenPosition == null)
                return;

            var drawableArgs = new DrawableClickedEventArgs(
                Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(),
                new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.NumTaps);

            clickedDrawable?.HandleClicked(drawableArgs);

            e.Handled = drawableArgs.Handled;

            return;
        }
        // Check for clicked mylocation
        else if (e.MapInfo?.Layer == MyLocationLayer)
        {
            if (e.MapInfo!.ScreenPosition == null)
                return;

            var args = new DrawableClickedEventArgs(
                Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(),
                new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.NumTaps);

            MyLocationLayer?.HandleClicked(args);

            e.Handled = args.Handled;

            return;
        }
    }

    private void HandlerLongTap(object? sender, TappedEventArgs e)
    {
        var args = new MapLongClickedEventArgs(Map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition).ToNative());
        MapLongClicked?.Invoke(this, args);

        if (args.Handled)
        {
            e.Handled = true;
        }
    }

    private void HandlerTap(object? sender, TappedEventArgs e)
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
                if (widget.Enabled && (widget.Envelope?.Contains(e.ScreenPosition) ?? false))
                {
                    if (widget.HandleWidgetTouched(Map.Navigator, e.ScreenPosition))
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }

            // Check, if we hit a drawable
            // Is there a drawable at this position
            var mapInfo = GetMapInfo(e.ScreenPosition);

            if (mapInfo?.Feature == null)
            {
                var args = new MapClickedEventArgs(Map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition).ToNative(), e.NumOfTaps);
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

    private void HandlerPinPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Map.Navigator.Viewport.ToExtent() is not null)
        {
            var fetchInfo = new FetchInfo(Map.Navigator.Viewport.ToSection(), Map?.CRS, ChangeType.Continuous);
            Map?.RefreshData(fetchInfo);
        }

        // Repaint map, because something could have changed
        RefreshGraphics();
    }

    private void HandlerDrawablePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Map.Navigator.Viewport.ToExtent() is not null)
        {
            var fetchInfo = new FetchInfo(Map.Navigator.Viewport.ToSection(), Map?.CRS, ChangeType.Continuous);
            Map?.RefreshData(fetchInfo);
        }

        // Repaint map, because something could have changed
        RefreshGraphics();
    }

    private void HandlerSizeChanged(object? sender, EventArgs e)
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
        Map?.Layers.Add(_mapDrawableLayer, _mapPinLayer, _mapCalloutLayer, MyLocationLayer);
    }

    /// <summary>
    /// Remove all layers that MapView uses
    /// </summary>
    private void RemoveLayers()
    {
        if (!_initialized)
            return;

        // Remove MapView layers
        Map?.Layers.Remove(MyLocationLayer, _mapCalloutLayer, _mapPinLayer, _mapDrawableLayer);
    }

    private void UpdateButtonPositions()
    {
        var newX = Width - ButtonMargin.Right - ButtonSize;
        var newY = ButtonMargin.Top;

        if (IsZoomButtonVisible)
        {
            _mapZoomInButton!.Envelope = new MRect(newX, newY, newX + ButtonSize, newY + ButtonSize);
            newY += ButtonSize;
            _mapZoomOutButton!.Envelope = new MRect(newX, newY, newX + ButtonSize, newY + ButtonSize);
            newY += ButtonSize + ButtonSpacing;
        }

        if (IsMyLocationButtonVisible)
        {
            _mapMyLocationButton!.Envelope = new MRect(newX, newY, newX + ButtonSize, newY + ButtonSize);
            newY += ButtonSize + ButtonSpacing;
        }

        if (IsNorthingButtonVisible)
        {
            _mapNorthingButton!.Envelope = new MRect(newX, newY, newX + ButtonSize, newY + ButtonSize);
        }

        RefreshGraphics();
    }

    private void RemoveButtons()
    {
        if (Map != null)
        {
            var widgets = Map.Widgets.ToList();
            widgets.Remove(_mapZoomInButton!);
            widgets.Remove(_mapZoomOutButton!);
            widgets.Remove(_mapMyLocationButton!);
            widgets.Remove(_mapNorthingButton!);
            Map.Widgets.Clear();
            Map.Widgets.AddRange(widgets);
        }

        RefreshGraphics();
    }

    private void CreateButtons()
    {
        _mapZoomInButton = _mapZoomInButton ?? CreateButton(0, 0, _pictZoomIn, (s, e) => { Map.Navigator.ZoomIn(); e.Handled = true; });
        _mapZoomInButton.Picture = _pictZoomIn;
        _mapZoomInButton.Enabled = IsZoomButtonVisible;
        Map!.Widgets.Add(_mapZoomInButton);

        _mapZoomOutButton = _mapZoomOutButton ?? CreateButton(0, 40, _pictZoomOut, (s, e) => { Map.Navigator.ZoomOut(); e.Handled = true; });
        _mapZoomOutButton.Picture = _pictZoomOut;
        _mapZoomOutButton.Enabled = IsZoomButtonVisible;
        Map!.Widgets.Add(_mapZoomOutButton);

        _mapMyLocationButton = _mapMyLocationButton ?? CreateButton(0, 88, _pictMyLocationNoCenter, (s, e) => { MyLocationFollow = true; e.Handled = true; });
        _mapMyLocationButton.Picture = _pictMyLocationNoCenter;
        _mapMyLocationButton.Enabled = IsMyLocationButtonVisible;
        Map!.Widgets.Add(_mapMyLocationButton);

        _mapNorthingButton = _mapNorthingButton ?? CreateButton(0, 136, _pictNorthing, (s, e) => { RunOnUIThread(() => Map.Navigator.RotateTo(0)); e.Handled = true; });
        _mapNorthingButton.Picture = _pictNorthing;
        _mapNorthingButton.Enabled = IsNorthingButtonVisible;
        Map!.Widgets.Add(_mapNorthingButton);

        UpdateButtonPositions();
    }

    private ButtonWidget CreateButton(float x, float y, SKPicture picture, Action<object?, WidgetTouchedEventArgs> action)
    {
        var result = new ButtonWidget
        {
            Picture = picture,
            Envelope = new MRect(x, y, x + ButtonSize, y + ButtonSize),
            Rotation = 0,
            Enabled = true,
        };
        result.WidgetTouched += (s, e) => action(s, e);
        result.PropertyChanged += (s, e) => RefreshGraphics();

        return result;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _mapCalloutLayer?.Dispose();
            _mapPinLayer?.Dispose();
            _mapDrawableLayer?.Dispose();
            _pictMyLocationNoCenter?.Dispose();
            _pictMyLocationCenter?.Dispose();
            _pictZoomIn?.Dispose();
            _pictZoomOut?.Dispose();
            _pictNorthing?.Dispose();
            MyLocationLayer?.Dispose();
        }
    }

    public void Reset()
    {
        Pins.Clear();
        Drawables.Clear();
        HideCallouts();
    }
}
