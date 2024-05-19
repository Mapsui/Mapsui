using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using System.Threading.Tasks;
using ExCSS;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.Widgets.BoxWidgets;
using Color = Mapsui.Styles.Color;
using HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment;
using VerticalAlignment = Mapsui.Widgets.VerticalAlignment;

namespace Mapsui.Tests.Common.Maps;

public class TouchPointSample : ISample
{
    private static Map _map;
    private TextBoxWidget _label;
    public string Name => "Touch Point";

    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public Map CreateMap()
    {
        _map = new Map
        {
            BackColor = Color.WhiteSmoke,
            CRS = "EPSG:3857",
        };

        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var memoryLayer = CreateMemoryLayer();
        _map.Layers.Add(memoryLayer);
        _label = CreateLabel(_map, HorizontalAlignment.Center, VerticalAlignment.Top, "Not Selected");
        _map.Widgets.Add(_label);
        memoryLayer.DataHasChanged();
        _map.Info += MapControl_Info;
        return _map;
    }

    private static ILayer CreateMemoryLayer()
    {
        var pinStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            Fill = new Brush(Color.Red),
            Outline = null
        };

        var features = new List<IFeature> { new PointFeature(SphericalMercator.FromLonLat(new MPoint(0, 0))) };

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
        if (e.MapInfo is { Feature: PointFeature, Layer: MemoryLayer })
        {
            if (_label.Text == "Not Selected")
            {
                _label.Text = "Selected";
            }
            else
            {
                _label.Text = "Not Selected";
            }
            _label.NeedsRedraw = true;
        }
    }

    private static TextBoxWidget CreateLabel(Map map,
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
}
