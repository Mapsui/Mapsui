using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;

namespace Mapsui.Layers
{
    public  class MemoryLayer : BaseLayer
    {
        public string Name { get; set; }
        public IProvider DataSource { get; set; }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            return DataSource.GetFeaturesInView(box, resolution);
        }

        public override BoundingBox Envelope
        {
            get
            {
                return DataSource.GetExtents();
            }
        }

        public override void AbortFetch()
        {
            // do nothing. This is not an async layer
        }

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            // do nothing. This is not an async layer
            OnDataChanged(new DataChangedEventArgs());
        }

        public override void ClearCache()
        {
            // do nothing. This is not an async layer
        }
    }
}
