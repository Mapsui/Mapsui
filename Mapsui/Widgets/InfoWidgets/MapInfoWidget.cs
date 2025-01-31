using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapsui.Widgets.InfoWidgets;

public class MapInfoWidget : TextBoxWidget
{
    private readonly Map _map;
    private readonly Func<IEnumerable<ILayer>> _layers;

    /// <summary>
    /// Widget displaying information about the feature at the current mouse position
    /// </summary>
    /// <param name="map">The map that is queried.</param>
    /// <param name="layers">The list of layers to filter.</param>
    public MapInfoWidget(Map map, IEnumerable<ILayer> layers)
        : this(map, () => layers)
    {
    }

    /// <summary>
    /// Widget displaying information about the feature at the current mouse position
    /// </summary>
    /// <param name="map">The map that is queried.</param>
    /// <param name="layersFilter">The filter to select the layers to query. The advantage of a filter is that 
    /// it can handle changes to the layer list later on.</param>
    public MapInfoWidget(Map map, Func<ILayer, bool> layersFilter)
        : this(map, () => map.Layers.Where(layersFilter))
    {
    }

    /// <summary>
    /// Widget displaying information about the feature at the current mouse position
    /// </summary>
    /// <param name="map">The map that is queried.</param>
    /// <param name="getMapInfoLayers">The method to retrieve the layers to query.</param>
    public MapInfoWidget(Map map, Func<IEnumerable<ILayer>> getMapInfoLayers)
    {
        // Todo: Avoid Map in the constructor. Perhaps the event args should have a GetMapInfoAsync method
        _map = map;
        _layers = getMapInfoLayers;
        _map.Info += Map_Info;

        VerticalAlignment = VerticalAlignment.Bottom;
        HorizontalAlignment = HorizontalAlignment.Left;
        Margin = new MRect(16);
        Padding = new MRect(10);
        CornerRadius = 4;
        BackColor = new Color(108, 117, 125);
        TextColor = Color.White;
    }

    private void Map_Info(object? sender, MapInfoEventArgs a)
    {
        var mapInfo = a.GetMapInfo(_layers());
        Text = FeatureToText(mapInfo.Feature);
        _map.RefreshGraphics();
        // Try to load async data
        Catch.Exceptions(async () =>
        {
            var info = await a.GetRemoteMapInfoAsync(_map.Layers.Where(t => t is ILayerFeatureInfo));
            var featureText = FeatureToText(info.Feature);
            if (!string.IsNullOrEmpty(featureText))
            {
                Text = featureText;
                _map.RefreshGraphics();
            }
        });
    }

    public Func<IFeature?, string> FeatureToText { get; set; } = (f) =>
    {
        if (f is null) return string.Empty;

        var result = new StringBuilder();

        result.Append("Info: ");
        foreach (var field in f.Fields)
            result.Append($"{field}: {f[field]} - ");
        result.Remove(result.Length - 2, 2);
        return result.ToString();
    };
}
