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
        private readonly long timeOfRequest;

        internal delegate void DataArrivedDelegate(IEnumerable<IFeature> features, object state = null);

        public FeatureFetcher(BoundingBox extent, double resolution, IProvider provider, DataArrivedDelegate dataArrived, long timeOfRequest = default(long))
        {
            this.dataArrived = dataArrived;
            this.extent = extent;
            this.provider = provider;
            this.resolution = resolution;
            this.timeOfRequest = timeOfRequest;
        }

        public void FetchOnThread()
        {
            lock (provider)
            {
                provider.Open();
                var features = provider.GetFeaturesInView(extent, resolution);
                provider.Close();
                if (dataArrived != null) dataArrived(features, timeOfRequest);
            }
        }
    }
}
