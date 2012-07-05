using System;
using System.Collections.Generic;
using System.Linq;
using SharpMap.Geometries;
using SharpMap.Providers;

namespace SharpMap.Fetcher
{
    class FeatureFetcher
    {
        private readonly BoundingBox extent;
        private readonly double resolution;
        private readonly DataArrivedDelegate dataArrived;
        private readonly IProvider provider;

        internal delegate void DataArrivedDelegate(IEnumerable<IFeature> features);

        public FeatureFetcher(BoundingBox extent, double resolution, IProvider provider, DataArrivedDelegate dataArrived)
        {
            this.dataArrived = dataArrived;
            this.extent = extent;
            this.provider = provider;
            this.resolution = resolution;
        }

        public void FetchOnThread()
        {
            lock (provider)
            {
                provider.Open();
                var features = provider.GetFeaturesInView(extent, resolution);
                provider.Close();
                if (dataArrived != null) dataArrived(features);
            }
        }
    }
}
