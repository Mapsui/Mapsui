using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable IDISP003
#pragma warning disable IDISP001

namespace Mapsui.Tests.Common.Maps;

public sealed class TouchPointSample : ISample, IDisposable
{
    private Map? _map;
    private TextBoxWidget? _label;
    private TextBoxWidget? _mousePosition;
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
        var memoryLayer = CreateMemoryLayer("Layer", Color.Red);
        _map.Layers.Add(memoryLayer);
        _map.Layers.Add(CreateMemoryLayer("Click Layer", Color.Blue, 0.3d, false));
        _label = CreateLabel(HorizontalAlignment.Center, VerticalAlignment.Top, "Not Selected");
        _map.Widgets.Add(_label);
        _mousePosition = CreateLabel(HorizontalAlignment.Center, VerticalAlignment.Bottom);
        _map.Widgets.Add(_mousePosition);
        memoryLayer.DataHasChanged();
        _map.Tapped += MapTapped;
        return _map;
    }

    private static MemoryLayer CreateMemoryLayer(string layerName, Color color, double scale = 1, bool createdPoint = true)
    {
        List<IFeature> features = createdPoint ? [new PointFeature(SphericalMercator.FromLonLat(new MPoint(0, 0)))] : [];

        return new MemoryLayer
        {
            Name = layerName,
            Features = features,
            Style = CreatePinStyle(color, scale)
        };
    }

    private static SymbolStyle CreatePinStyle(Color color, double scale) => new()
    {
        SymbolType = SymbolType.Ellipse,
        Fill = new Brush(color),
        Outline = null,
        SymbolScale = scale
    };

    private void MapTapped(object? s, MapEventArgs e)
    {
        var mapInfo = e.GetMapInfo(e.Map.Layers.Where(l => l.Name == "Layer"));
        _mousePosition!.Text = $"X: {Convert.ToInt32(mapInfo.ScreenPosition.X)}, Y: {Convert.ToInt32(mapInfo.ScreenPosition.Y)}";
        _mousePosition.NeedsRedraw = true;
        var clickLayer = e.Map.Layers.OfType<MemoryLayer>().First(l => l.Name == "Click Layer");
        var features = (List<IFeature>)clickLayer.Features;
        features.Add(new PointFeature(mapInfo.WorldPosition.X, mapInfo.WorldPosition.Y));
        clickLayer.DataHasChanged();
        if (mapInfo is { Feature: PointFeature, Layer: MemoryLayer })
        {
            _label!.Text = _label.Text == "Not Selected" ? "Selected" : "Not Selected";
            _label.NeedsRedraw = true;
            e.Handled = true;
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
        _map?.Dispose();
    }
}
