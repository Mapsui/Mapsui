using System.Collections.Generic;
using System.Linq;
using Mapsui.GeometryLayer;

namespace Mapsui.Extensions
{
    public static class FeatureExtensions
    {
        public static GeometryFeature Copy(this GeometryFeature original)
        {
            var geometryFeature = new GeometryFeature();
            geometryFeature.Geometry = original.Geometry.Copy();
            geometryFeature.RenderedGeometry =
                original.RenderedGeometry.ToDictionary(entry => entry.Key, entry => entry.Value);
            geometryFeature.Styles = original.Styles.ToList();
            foreach (var field in original.Fields)
                geometryFeature[field] = original[field];
            return geometryFeature;
        }

        public static IEnumerable<GeometryFeature> Copy(this IEnumerable<GeometryFeature> original)
        {
            return original.Select(f => f.Copy()).ToList();
        }
    }
}