using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Rendering.Xaml
{
    /// <summary>
    /// Wrapper around a feature provider that returns a rasterized image of the features.
    /// </summary>
    ///
    [Obsolete("Use RasterizingLayer as an alternative approach", true)]
    public class RasterizingProvider : IProvider
    {
        public string CRS { get; set; }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            throw new NotImplementedException();
        }
        
        public BoundingBox GetExtents()
        {
            throw new NotImplementedException();
        }
    }
}