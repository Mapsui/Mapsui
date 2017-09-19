using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    public  class MemoryLayer : BaseLayer
    {
        public IProvider DataSource { get; set; }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            // Safeguard in case BoundingBox is null, most likely due to no features in layer
            if (box == null) { return new List<IFeature>(); }

            var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            return DataSource.GetFeaturesInView(biggerBox, resolution);
        }

        public override BoundingBox Envelope => DataSource?.GetExtents();

        public override void AbortFetch()
        {
            // do nothing. This is not an async layer
        }

        public override void ViewChanged(bool majorChange, BoundingBox extent, double resolution)
        {
            //The MemoryLayer always has it's data ready so can fire a DataChanged event immediately so that listeners can act on it.
            Task.Run(() => OnDataChanged(new DataChangedEventArgs()));
        }

        public override void ClearCache()
        {
            // do nothing. This is not an async layer
        }
    }
}
