using System;
using System.Collections.Generic;
using GeoJSON.Net.Feature;
using Mapsui.Styles;

namespace Mapsui.VectorTiles.Extensions
{
    static class FeatureExtensions
    {
        private static readonly Random Random = new Random(4967443);

        public static IEnumerable<Providers.Feature> ToMapsui(
            this IEnumerable<FeatureCollection> geoJsonFeatureCollections)
        {
            // Not returning an IEnumerable<IEnumerable<FeatureCollection>> here because I don't see the added value (yet?)
            var result = new List<Providers.Feature>();

            foreach (var geoJsonFeatureCollection in geoJsonFeatureCollections)
            {
                result.AddRange(geoJsonFeatureCollection.ToMapsui());
            }

            return result;
        }

        public static IEnumerable<Providers.Feature> ToMapsui(this FeatureCollection geoJsonFeatureCollection)
        {
            var result = new List<Providers.Feature>();
            foreach (var geoJsonFeature in geoJsonFeatureCollection.Features)
            {
                var feature = geoJsonFeature.ToMapsui();
                if (feature == null) continue;
                result.Add(feature);
            }
            return result;
        }

        public static Providers.Feature ToMapsui(this Feature geoJsonFeature)
        {
            var feature = new Providers.Feature {Geometry = geoJsonFeature.Geometry.ToMapsui()};

            if (feature.Geometry == null) return null;

            foreach (var field in geoJsonFeature.Properties)
            {
                feature[field.Key] = field.Value;
            }
            
            var color = Color.FromArgb(255, Random.Next(256), Random.Next(256), Random.Next(256));
            feature.Styles.Add(new VectorStyle { Fill = new Brush(color) });

            return feature;
        }
    }
}
