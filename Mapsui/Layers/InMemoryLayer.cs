using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Layers
{
    public  class InMemoryLayer : BaseLayer
    {
        public MemoryProvider MemoryProvider { get; private set; }

        public InMemoryLayer()
        {
            MemoryProvider = new MemoryProvider();
        }

        public InMemoryLayer(MemoryProvider memoryProvider)
        {
            MemoryProvider = memoryProvider;
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            return MemoryProvider.GetFeaturesInView(box, resolution);
        }

        public override void AbortFetch()
        {
            // do nothing. No fetching is done
        }

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            // do nothing. No fetching is done
        }

        public override event DataChangedEventHandler DataChanged;

        public override void ClearCache()
        {
            MemoryProvider.Clear();
        }
    }
}
