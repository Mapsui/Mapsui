using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    /// <summary>
    /// A layer to use, when DataSource doesn't fetch anything because it is already in memory
    /// </summary>
    public  class MemoryLayer : BaseLayer
    {
        /// <summary>
        /// Create a new layer
        /// </summary>
        public MemoryLayer() : this("MemoryLayer") { }

        /// <summary>
        /// Create layer with name
        /// </summary>
        /// <param name="layername">Name to use for layer</param>
        public MemoryLayer(string layername) : base(layername) { }

        public IProvider DataSource { get; set; }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            // Safeguard in case BoundingBox is null, most likely due to no features in layer
            if (box == null) { return new List<IFeature>(); }

            var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            return DataSource.GetFeaturesInView(biggerBox, resolution);
        }

        public override void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            // RefreshData needs no implementation for the MemoryLayer
        }

        public override BoundingBox Envelope => DataSource?.GetExtents();
    }
}
