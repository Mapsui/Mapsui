using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    /// <summary>
    /// A layer to use, when DataSource doesn't fetch anything because it is already in memory
    /// </summary>
    public class MemoryLayer : BaseLayer
    {
        public IEnumerable<IFeature> Features { get; set; } = new List<IFeature>();

        /// <summary>
        /// Create a new layer
        /// </summary>
        public MemoryLayer() : this("MemoryLayer") { }

        /// <summary>
        /// Create layer with name
        /// </summary>
        /// <param name="layerName">Name to use for layer</param>
        public MemoryLayer(string layerName) : base(layerName) { }

        // Unlike other Layers the MemoryLayer has a CRS field. This is because the 
        // MemoryLayer calls its provider from the GetFeatures method instead of the 
        // RefreshData method. The GetFeatures arguments do not have a CRS argument.
        // This field allows a workaround for when projection is needed.
        public string? CRS { get; set; }

        public override IEnumerable<IFeature> GetFeatures(MRect? rect, double resolution)
        {
            // Safeguard in case BoundingBox is null, most likely due to no features in layer
            if (rect == null) { return new List<IFeature>(); }

            var biggerRect = rect.Grow(
                    SymbolStyle.DefaultWidth * 2 * resolution,
                    SymbolStyle.DefaultHeight * 2 * resolution);
            var fetchInfo = new FetchInfo(biggerRect, resolution, CRS);

            return Features.Where(f => f.Extent?.Intersects(biggerRect) == true);
        }

        public override void RefreshData(FetchInfo fetchInfo)
        {
            // RefreshData needs no implementation for the MemoryLayer. Calling OnDataChanged here
            // would trigger an extra needless render iteration.
            // If a user changed the data in the provider and needs to update the graphics
            // DataHasChanged should be called.
        }

        public override MRect? Extent
        {
            get
            {
                MRect? result = null;
                foreach (MRect extent in Features.Where(f => f.Extent is not null).Select(f => f.Extent!))
                {
                    if (result is null)
                        result = extent;
                    else
                        result = result.Join(extent);
                }
                return result;
            }
        }
    }
}
