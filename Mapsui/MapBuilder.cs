using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using System;
using System.Collections.Generic;

namespace Mapsui;

public class MapBuilder
{
    readonly List<AddLayer> _layerFactories = [];
    readonly List<AddLayer> _baseLayerFactories = [];
    readonly List<AddWidget> _widgetFactories = [];
    readonly List<ConfigureMap> _mapConfigurators = [];

    public MapBuilder WithMapConfiguration(ConfigureMap configureMap)
    {
        _mapConfigurators.Add(configureMap);
        return this;
    }

    public MapBuilder WithZoomButtons()
    {
        _widgetFactories.Add((m) => new ZoomInOutWidget() { Margin = new MRect(16, 32) });
        return this;
    }

    public MapBuilder WithBaseLayer(AddLayer layerFactory)
    {
        _baseLayerFactories.Add(layerFactory);
        return this;
    }

    public MapBuilder WithLayer(AddLayer layerFactory, ConfigureLayer configureLayer)
    {
        _layerFactories.Add((m) =>
        {
            var layer = layerFactory(m);
            configureLayer(layer);
            return layer;
        });
        return this;
    }

    public MapBuilder WithLayer(AddLayer layerFactory)
        => WithLayer(layerFactory, (l) => { });

    public MapBuilder WithPinWithCalloutLayer(IEnumerable<IFeature>? features = null)
        => WithLayer(m => CreateLayerWithPinWithCallout(m, features ?? []), (l) => { });

    public MapBuilder WithMapCRS(string crs)
    {
        _mapConfigurators.Add((m) => m.CRS = crs);
        return this;
    }

    public MapBuilder WithWidget(AddWidget widgetFactory, ConfigureWidget configureWidget)
    {
        _widgetFactories.Add((m) =>
        {
            var widget = widgetFactory(m);
            configureWidget(widget);
            return widget;
        });
        return this;
    }

    public MapBuilder WithWidget(AddWidget widgetFactory)
        => WithWidget(widgetFactory, (w) => { });

    public Map Build()
    {
        var map = new Map();

        foreach (var layerFactory in _layerFactories)
            map.Layers.Add(layerFactory(map));

        foreach (var widgetFactory in _widgetFactories)
            map.Widgets.Add(widgetFactory(map));

        foreach (var mapConfigurator in _mapConfigurators)
            mapConfigurator(map);

        return map;
    }

    public delegate ILayer AddLayer(Map map);
    public delegate void ConfigureMap(Map map);
    public delegate IWidget AddWidget(Map map);
    public delegate void ConfigureLayer(ILayer layer);
    public delegate void ConfigureWidget(IWidget widget);
    public delegate void TapFeature(Action<IFeature> tapFeature);

    private static MemoryLayer CreateLayerWithPinWithCallout(Map map, IEnumerable<IFeature> features)
    {
        map.Info += (sender, args) =>
        {
            var feature = args.MapInfo.Feature;
            if (feature is null)
                return;

            var enabled = feature["enabled"]?.ToString() == "True";
            feature["enabled"] = (!enabled).ToString();
        };

        return new()
        {
            IsMapInfoLayer = true,
            Features = features,
            Style = new StyleCollection
            {
                Styles =
                {
                    new SymbolStyle
                    {
                        ImageSource = "embedded://Mapsui.Resources.Images.Pin.svg",
                        SymbolOffset = new RelativeOffset(0.0, 0.5), // The point at the bottom should be at the location
                        SvgFillColor = Color.CornflowerBlue,
                        SvgStrokeColor = Color.Black,
                        SymbolScale = 1,
                    },
                    new ThemeStyle(f =>
                    {
                        return new CalloutStyle()
                        {
                            Enabled = f["enabled"]?.ToString() == "True",
                            SymbolOffset = new Offset(0, 52),
                            TitleFont = { FontFamily = null, Size = 24, Italic = false, Bold = true },
                            TitleFontColor = Color.Black,
                            Type = CalloutType.Single,
                            MaxWidth = 120,
                            Title = f["Name"]!.ToString()
                        };
                    })
                }
            }
        };
    }
}
