using Mapsui.Animations;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Special;

public class AddPinsInEmptyMapSample : ISample
{
    public string Name => "AddPinsInEmptyMap";
    public string Category => "Special";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    private static Map CreateMap()
    {
        var features = new List<IFeature>();

        var map = new Map();
        map.Layers.Add(CreatePinLayer(features));
        var extent = new MRect(-50000, -50000, 50000, 5000);
        map.Navigator.OverridePanBounds = extent;
        map.Navigator.ZoomToBox(extent);
        map.Tapped += (m, e) =>
        {
            var layer = e.Map.Layers.OfType<MemoryLayer>().First();
            // Add a point to the layer using the Info position
            var point = new Point(e.WorldPosition.X, e.WorldPosition.Y);
            features.Add(new GeometryFeature
            {
                Geometry = new Point(e.WorldPosition.X, e.WorldPosition.Y)
            });
            e.Map.Navigator.ZoomToBox(new MRect(point.X - 5000, point.Y - 5000, point.X + 5000, point.Y + 5000), duration: 1000, easing: Easing.CubicOut);
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
