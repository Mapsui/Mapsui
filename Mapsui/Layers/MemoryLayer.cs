using Mapsui.Providers;
using System.Collections.Generic;
using Mapsui.Extensions;
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
        /// <param name="layerName">Name to use for layer</param>
        public MemoryLayer(string layerName) : base(layerName) { }

        public IProvider<IFeature> DataSource { get; set; }

        public override IEnumerable<IFeature> GetFeaturesInView(MRect box, double resolution)
        {
            // Safeguard in case BoundingBox is null, most likely due to no features in layer
            if (box == null) { return new List<IFeature>(); }

            var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            return DataSource.GetFeaturesInView(biggerBox.ToBoundingBox(), resolution);
        }

        public override void RefreshData(MRect extent, double resolution, ChangeType changeType)
        {
            // RefreshData needs no implementation for the MemoryLayer. Calling OnDataChanged here
            // would trigger an extra needless render iteration.
            // If a user changed the data in the provider and needs to update the graphics
            // DataHasChanged should be called.
        }

        public override MRect Envelope => DataSource?.GetExtents().ToMRect();
    }
}
