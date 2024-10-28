using System;
using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.Widgets.BoxWidgets;
using Color = Mapsui.Styles.Color;
using HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment;
using VerticalAlignment = Mapsui.Widgets.VerticalAlignment;
#pragma warning disable IDISP003
#pragma warning disable IDISP001

namespace Mapsui.Tests.Common.Maps;

public sealed class TouchPointSample : ISample, IDisposable
{
    private Map? _map;
    private TextBoxWidget? _label;
    private TextBoxWidget? _mousePosition;
    private MemoryLayer? _clickMemoryLayer;
    public string Name => "Touch Point";

    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    private Map CreateMap()
    {
        _map = new Map
        {
            BackColor = Color.WhiteSmoke,
            CRS = "EPSG:3857",
        };

        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var memoryLayer = CreateMemoryLayer(Color.Red);
        _map.Layers.Add(memoryLayer);
        _clickMemoryLayer = CreateMemoryLayer(Color.Blue, 0.3d, false);
        _map.Layers.Add(_clickMemoryLayer);
        _label = CreateLabel(HorizontalAlignment.Center, VerticalAlignment.Top, "Not Selected");
        _map.Widgets.Add(_label);
        _mousePosition = CreateLabel(HorizontalAlignment.Center, VerticalAlignment.Bottom);
        _map.Widgets.Add(_mousePosition);
        memoryLayer.DataHasChanged();
        _map.Info += MapControl_Info;
        return _map;
    }

    private static MemoryLayer CreateMemoryLayer(Color color, double scale = 1, bool createdPoint = true)
    {
        var pinStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            Fill = new Brush(color),
            Outline = null,
            SymbolScale = scale
        };

        List<IFeature> features = createdPoint ? [new PointFeature(SphericalMercator.FromLonLat(new MPoint(0, 0)))] : [];

        var memoryLayer = new MemoryLayer
        {
            Name = "Key",
            IsMapInfoLayer = true,
            Features = features,
            Style = pinStyle
        };

        return memoryLayer;
    }

    private void MapControl_Info(object? sender, MapInfoEventArgs e)
    {
        _mousePosition!.Text = $"X: {Convert.ToInt32(e.MapInfo.ScreenPosition.X)}, Y: {Convert.ToInt32(e.MapInfo.ScreenPosition.Y)}";
        _mousePosition.NeedsRedraw = true;
        var features = (List<IFeature>)_clickMemoryLayer!.Features;
        features.Add(new PointFeature(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y));
        _clickMemoryLayer.DataHasChanged();
        if (e.MapInfo is { Feature: PointFeature, Layer: MemoryLayer })
        {
            _label!.Text = _label.Text == "Not Selected" ? "Selected" : "Not Selected";
            _label.NeedsRedraw = true;
        }
    }

    private static TextBoxWidget CreateLabel(
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment verticalAlignment = VerticalAlignment.Top,
        string text = "")
    {
        return new TextBoxWidget
        {
            Text = text,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            Margin = new MRect(10),
            Padding = new MRect(4),
            CornerRadius = 4,
            BackColor = new Color(108, 117, 125),
            TextColor = Color.White,
        };
    }

    public void Dispose()
    {
        _clickMemoryLayer?.Dispose();
        _map?.Dispose();
    }
}
