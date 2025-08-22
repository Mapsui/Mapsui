using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class AddPinsSample : ISample
{
    public string Name => "Add Pins";
    public string Category => "Demo";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    private static Map CreateMap()
    {
        var features = new List<IFeature>();

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePinLayer(features));
        map.Tapped += (m, e) =>
        {
            var layer = e.Map.Layers.OfType<MemoryLayer>().First();
            // Add a point to the layer using the Info position
            features.Add(new GeometryFeature
            {
                Geometry = new Point(e.WorldPosition.X, e.WorldPosition.Y)
            });
            // The MemoryLayer needs to update the changed features.
            layer.FeaturesWereModified();
            // To notify the map that a redraw is needed.
            layer.DataHasChanged();
            e.Handled = true;
        };
        return map;
    }

    private static MemoryLayer CreatePinLayer(IEnumerable<IFeature> features) => new()
    {
        Name = "Pin Layer",
        Features = features,
        Style = ImageStyles.CreatePinStyle()
    };
}
