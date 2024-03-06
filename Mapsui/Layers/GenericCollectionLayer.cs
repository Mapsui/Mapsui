using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Styles;

namespace Mapsui.Layers;

/// <summary>
/// The GenericCollectionLayer uses a T of IEnumerable of IFeature
/// </summary>
public class GenericCollectionLayer<T> : BaseLayer where T : IEnumerable<IFeature>, new()
{
    public GenericCollectionLayer() : base(nameof(MemoryLayer)) { }

    public T Features { get; set; } = new T();

    public override IEnumerable<IFeature> GetFeatures(MRect? rect, double resolution)
    {
        // Safeguard in case BoundingBox is null, most likely due to no features in layer
        if (rect == null) { return new List<IFeature>(); }

        var biggerRect = rect.Grow(
                SymbolStyle.DefaultWidth * 2 * resolution,
                SymbolStyle.DefaultHeight * 2 * resolution);

        return Features.Where(f => f.Extent?.Intersects(biggerRect) == true);
    }

    public override MRect? Extent => Features.GetExtent();
}
