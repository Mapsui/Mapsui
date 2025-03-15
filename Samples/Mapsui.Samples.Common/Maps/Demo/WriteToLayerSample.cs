using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class WriteToLayerSample : ISample
{
    public string Name => "Add Pins";
    public string Category => "Demo";

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable")]
    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    private static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateGenericCollectionLayer());
        map.Tapped += (m, e) =>
        {
            var layer = (GenericCollectionLayer<List<IFeature>>)e.Map.Layers.First(l => l.Name == "GenericCollectionLayer");
            // Add a point to the layer using the Info position
            layer?.Features.Add(new GeometryFeature
            {
                Geometry = new Point(e.WorldPosition.X, e.WorldPosition.Y)
            });
            // To notify the map that a redraw is needed.
            layer?.DataHasChanged();
            e.Handled = true;
        };
        return map;
    }

    private static GenericCollectionLayer<List<IFeature>> CreateGenericCollectionLayer()
    {
        return new GenericCollectionLayer<List<IFeature>>
        {
            Name = "GenericCollectionLayer",
            Style = ImageStyles.CreatePinStyle()
        };
    }
}
