using Mapsui.Fetcher;
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

        private IProvider _dataSource;

        public IProvider DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                if (_dataSource != value)
                {
                    _dataSource = value;
                    OnDataChanged(new DataChangedEventArgs());
                }
            }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            // Safeguard in case BoundingBox is null, most likely due to no features in layer
            if (box == null) { return new List<IFeature>(); }

            var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            return _dataSource.GetFeaturesInView(biggerBox, resolution);
        }

        public override void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            //The MemoryLayer always has it's data ready so can fire a DataChanged event immediately so that listeners can act on it.
            OnDataChanged(new DataChangedEventArgs(false));
        }

        public override BoundingBox Envelope => _dataSource?.GetExtents();
    }
}
