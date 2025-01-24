using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using System;
using Mapsui.Widgets.BoxWidgets;
using System.Collections.Generic;

namespace Mapsui.Widgets.InfoWidgets;

public class RulerWidget(Map map) : BaseWidget
{
    public enum TapType
    {
        Down,
        Drag,
        Hover,
        Up
    }

    private readonly Map _map = map;

    public Color Color { get; set; } = new Color(192, 30, 20, 255);
    public Color ColorOfBeginAndEndDots { get; set; } = new Color(192, 30, 20, 128);
    public MPoint? StartPosition { get; internal set; }
    public MPoint? CurrentPosition { get; internal set; }
    public double? DistanceInKilometers { get; internal set; }
    public bool IsActive { get; set; }

    public TextBoxWidget TextBox { get; set; } = new TextBoxWidget
    {
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        BackColor = new Color(128, 128, 0, 128),
        Padding = new MRect(6, 4),
    };

    public event EventHandler<MeasureWidgetUpdatedEventArgs>? DistanceUpdated = null;

    public override bool OnPointerPressed(Navigator navigator, WidgetEventArgs e)
    {
        CurrentPosition = null;
        StartPosition = _map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition);
        _map.RefreshGraphics();
        return true;
    }

    public override bool OnPointerMoved(Navigator navigator, WidgetEventArgs e)
    {
        if (!e.LeftButton)
            return false; // Not dragging.

        CurrentPosition = _map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition);
        DistanceInKilometers = GetDistance(StartPosition, CurrentPosition);
        DistanceUpdated?.Invoke(this, new MeasureWidgetUpdatedEventArgs(e.LeftButton ? TapType.Drag : TapType.Hover));
        _map.RefreshGraphics();
        return true;
    }

    public override bool OnPointerReleased(Navigator navigator, WidgetEventArgs e)
    {
        DistanceUpdated?.Invoke(this, new MeasureWidgetUpdatedEventArgs(TapType.Up));
        _map.RefreshGraphics();
        return true;
    }

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        if (e.TapType == Manipulations.TapType.Single)
        {
            StartPosition = _map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition);
            CurrentPosition = null;
            DistanceUpdated?.Invoke(this, new MeasureWidgetUpdatedEventArgs(TapType.Down));
            _map.RefreshGraphics();
        }
        return true;
    }

    public void Reset()
    {
        StartPosition = null;
        CurrentPosition = null;
    }

    private static double? GetDistance(MPoint? fromPosition, MPoint? toPosition)
    {
        if (fromPosition == null || toPosition == null)
            return null;

        var fromLatLon = SphericalMercator.ToLonLat(fromPosition);
        var toLatLon = SphericalMercator.ToLonLat(toPosition);

        return Utilities.Haversine.Distance(fromLatLon.X, fromLatLon.Y, toLatLon.X, toLatLon.Y);
    }

    public (IFeature? startFeature, IFeature? currentFeature) SnapToFeature(Func<MPoint?, IFeature?> getFeaturesToSnapTo)
    {
        var result = new List<IFeature>();

        var startFeature = getFeaturesToSnapTo(StartPosition);
        if (startFeature != null)
        {
            result.Add(startFeature);
            StartPosition = startFeature?.Extent?.Centroid;
        };

        var currentFeature = getFeaturesToSnapTo(CurrentPosition);
        if (currentFeature != null)
        {
            result.Add(currentFeature);
            CurrentPosition = currentFeature?.Extent?.Centroid ?? CurrentPosition;
        }

        return (startFeature, currentFeature);
    }

    public class MeasureWidgetUpdatedEventArgs(TapType tapType) : EventArgs
    {
        public TapType TapType { get; } = tapType;
    }
}
