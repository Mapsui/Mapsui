using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.UI.Maui.Extensions;
using Mapsui.UI.Objects;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Maui;

/// <summary>
/// Class, that uses the API of the original Xamarin.Forms MapView
/// </summary>
public class MapView : MapControl, INotifyPropertyChanged, IEnumerable<Pin>
{
    private const string _calloutLayerName = "Callouts";
    private const string _pinLayerName = "Pins";
    private const string _drawableLayerName = "Drawables";
    private readonly ObservableMemoryLayer<Callout> _mapCalloutLayer = new(f => f.Feature) { Name = _calloutLayerName, IsMapInfoLayer = true };
    private readonly ObservableMemoryLayer<Pin> _mapPinLayer = new(f => f.Feature) { Name = _pinLayerName, IsMapInfoLayer = true };
    private readonly ObservableMemoryLayer<Drawable> _mapDrawableLayer = new(f => f.Feature) { Name = _drawableLayerName, IsMapInfoLayer = true };
    private ImageButtonWidget? _mapZoomInButton;
    private ImageButtonWidget? _mapZoomOutButton;
    private ImageButtonWidget? _mapMyLocationButton;
    private ImageButtonWidget? _mapNorthingButton;
    private readonly ObservableRangeCollection<Pin> _pins = [];
    private readonly ObservableRangeCollection<Drawable> _drawables = [];
    private readonly ObservableRangeCollection<Callout> _callouts = [];
    private readonly MapTappedWidget _mapTappedWidget;

    public MapView()
    {
        _mapTappedWidget = new MapTappedWidget(HandlerTap);
        MyLocationFollow = false;

        IsClippedToBounds = true;

        MyLocationLayer.MapView = this;

        // Get defaults from MapControl
        RotationLock = Map.Navigator.RotationLock;
        ZoomLock = Map.Navigator.ZoomLock;
        PanLock = Map.Navigator.PanLock;

        // Add some events to _mapControl
        Map.Navigator.ViewportChanged += HandlerViewportChanged;
        Info += (s, e) => HandlerInfo(e);
        SizeChanged += HandlerSizeChanged;

        // Add MapView layers to Map
        AddLayers();
        AddWidgets();

        // Add some events to _mapControl.Map.Layers
        Map.Layers.Changed += HandleLayersChanged;

        CreateButtons();

        _pins.CollectionChanged += HandlerPinsOnCollectionChanged;
        _drawables.CollectionChanged += HandlerDrawablesOnCollectionChanged;

        _mapCalloutLayer.ObservableCollection = _callouts;
        _mapCalloutLayer.Style = null;  // We don't want a global style for this layer

        _mapPinLayer.ObservableCollection = _pins;
        _mapPinLayer.Style = null;  // We don't want a global style for this layer

        _mapDrawableLayer.ObservableCollection = _drawables;
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
    public Objects.MyLocationLayer MyLocationLayer { get; } = new() { Enabled = true };

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
    /// Get callouts on map (those that are currently visible).
    /// </summary>
    public IReadOnlyList<Callout> GetCallouts() { return _callouts.ToList(); }

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
    public IList<Drawable> Drawables => _drawables;

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
    /// Enable panning
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
                _mapMyLocationButton!.ImageSource = "embedded://Mapsui.UI.Maui.Images.LocationCenter.svg";
                Map.Navigator.CenterOn(MyLocationLayer.MyLocation.ToMapsui());
            }
            else
            {
                _mapMyLocationButton!.ImageSource = "embedded://Mapsui.UI.Maui.Resources.Images.LocationNoCenter.svg";
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
                AddWidgets();

                // Remove widget buttons and readd them
                RemoveButtons();
                CreateButtons();

                // Add event handlers
                Map.Navigator.ViewportChanged += HandlerViewportChanged;
                Info += (s, e) => HandlerInfo(e);
            }
        }
    }

    #region Handlers

    /// <summary>
    /// Viewport of map has changed
    /// </summary>
    /// <param name="sender">Viewport of this event</param>
    /// <param name="e">Event arguments containing what changed</param>
    private void HandlerViewportChanged(object? sender, ViewportChangedEventArgs e)
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
        var localRemovedLayers = args.RemovedLayers?.ToList() ?? [];
        var localAddedLayers = args.AddedLayers?.ToList() ?? [];
        var movedLayers = args.MovedLayers?.ToList() ?? [];

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

    private void HandlerInfo(MapInfoEventArgs e)
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

                if (e.MapInfo?.ScreenPosition is null)
                    return;

                var pinArgs = new PinClickedEventArgs(clickedPin, Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(), e.TapType);

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

            var calloutArgs = new CalloutClickedEventArgs(clickedCallout,
                Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(),
                new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.TapType);

            clickedCallout?.HandleCalloutClicked(this, calloutArgs);

            e.Handled = calloutArgs.Handled;

            return;
        }
        // Check for clicked drawables
        else if (e.MapInfo?.Layer == _mapDrawableLayer)
        {
            var drawables = _drawables.ToList();

            foreach (var rec in e.MapInfo.MapInfoRecords)
            {
                foreach (var drawable in drawables)
                {
                    if (!drawable.IsClickable)
                        continue;
                    if (drawable.Feature?.Equals(rec.Feature) ?? false)
                    {
                        var drawableArgs = new DrawableClickedEventArgs(
                            Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(),
                            new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.TapType);

                        drawable?.HandleClicked(drawableArgs);

                        e.Handled = drawableArgs.Handled;

                        if (e.Handled)
                            return;
                    }
                }
            }
        }
        // Check for clicked myLocation
        else if (e.MapInfo?.Layer == MyLocationLayer)
        {
            var args = new DrawableClickedEventArgs(
                Map.Navigator.Viewport.ScreenToWorld(e.MapInfo!.ScreenPosition).ToNative(),
                new Point(e.MapInfo.ScreenPosition.X, e.MapInfo.ScreenPosition.Y), e.TapType);

            MyLocationLayer?.HandleClicked(args);

            e.Handled = args.Handled;

            return;
        }
    }

    private bool HandlerTap(WidgetEventArgs e)
    {
        var handled = false;
        var screenPosition = e.Position;

        if (Map != null)
        {
            // Check if we hit a drawable/pin/callout etc
            var mapInfo = GetMapInfo(screenPosition);

            var mapInfoEventArgs = new MapInfoEventArgs(mapInfo, e.TapType, handled);

            HandlerInfo(mapInfoEventArgs);

            handled = mapInfoEventArgs.Handled;

            if (!handled)
            {
                // if nothing else was hit, then we hit the map
                var args = new MapClickedEventArgs(Map.Navigator.Viewport.ScreenToWorld(screenPosition).ToNative(), e.TapType);
                MapClicked?.Invoke(this, args);

                if (args.Handled)
                {
                    handled = true;
                    return handled;
                }

                // Event isn't handled up to now.
                // Than look, what we could do.

                return handled;
            }
        }

        return handled;
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
        // Add MapView layers
        Map?.Layers.Add([_mapDrawableLayer, _mapPinLayer, _mapCalloutLayer, MyLocationLayer]);
    }

    /// <summary> Add Default Widgets </summary>
    private void AddWidgets()
    {
        if (Map != null && !Map.Widgets.Contains(this._mapTappedWidget))
            // Add MapView widgets
            Map.Widgets.Add(this._mapTappedWidget);
    }

    /// <summary>
    /// Remove all layers that MapView uses
    /// </summary>
    private void RemoveLayers()
    {
        // Remove MapView layers
        if (Map?.Layers.Count >= 4)
        {
            try
            {
                Map?.Layers.Remove([MyLocationLayer, _mapCalloutLayer, _mapPinLayer, _mapDrawableLayer]);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Layers could not be removed", ex);
            }
        }
    }

    private void UpdateButtonPositions()
    {
        var newX = Width - ButtonMargin.Right - ButtonSize;
        var newY = ButtonMargin.Top;

        if (IsZoomButtonVisible)
        {
            _mapZoomInButton!.Position = new MPoint(newX, newY);
            newY += ButtonSize;
            _mapZoomOutButton!.Position = new MPoint(newX, newY);
            newY += ButtonSize + ButtonSpacing;
        }

        if (IsMyLocationButtonVisible)
        {
            _mapMyLocationButton!.Position = new MPoint(newX, newY);
            newY += ButtonSize + ButtonSpacing;
        }

        if (IsNorthingButtonVisible)
        {
            _mapNorthingButton!.Position = new MPoint(newX, newY);
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
        _mapZoomInButton ??= CreateButton(0, 0, "embedded://Mapsui.UI.Maui.Images.ZoomIn.svg", (s, e) => { Map.Navigator.ZoomIn(); return true; });
        _mapZoomInButton.Enabled = IsZoomButtonVisible;
        Map!.Widgets.Add(_mapZoomInButton);

        _mapZoomOutButton ??= CreateButton(0, 40, "embedded://Mapsui.UI.Maui.Images.ZoomOut.svg", (s, e) => { Map.Navigator.ZoomOut(); return true; });
        _mapZoomOutButton.Enabled = IsZoomButtonVisible;
        Map!.Widgets.Add(_mapZoomOutButton);

        _mapMyLocationButton ??= CreateButton(0, 88, "embedded://Mapsui.UI.Maui.Images.LocationCenter.svg", (s, e) => { MyLocationFollow = true; return true; });
        _mapMyLocationButton.Enabled = IsMyLocationButtonVisible;
        Map!.Widgets.Add(_mapMyLocationButton);

        _mapNorthingButton ??= CreateButton(0, 136, "embedded://Mapsui.UI.Maui.Images.RotationZero.svg", (s, e) => { RunOnUIThread(() => Map.Navigator.RotateTo(0)); return true; });
        _mapNorthingButton.Enabled = IsNorthingButtonVisible;
        Map!.Widgets.Add(_mapNorthingButton);

        UpdateButtonPositions();
    }

    private ImageButtonWidget CreateButton(
        float x, float y, string imageSource, Func<ImageButtonWidget, WidgetEventArgs, bool> tapped) => new()
        {
            ImageSource = imageSource,
            HorizontalAlignment = Widgets.HorizontalAlignment.Absolute,
            VerticalAlignment = Widgets.VerticalAlignment.Absolute,
            Position = new MPoint(x, y),
            Width = ButtonSize,
            Height = ButtonSize,
            Rotation = 0,
            Enabled = true,
            Tapped = tapped
        };

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _mapCalloutLayer?.Dispose();
            _mapPinLayer?.Dispose();
            _mapDrawableLayer?.Dispose();
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
