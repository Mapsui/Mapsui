using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class CirclesSample
    {
        private static Random _random = new Random(0);

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(new Layer
            {
                DataSource = CreateProviderWithRandomPoints(map.Envelope),
                Style = new VectorStyle { Fill = new Brush(Color.White), Outline = new Pen { Color = Color.Black } },
            });
            return map;
        }

        public static MemoryProvider CreateProviderWithRandomPoints(BoundingBox envelope, int count = 100)
        {
            return new MemoryProvider(CreateFeatures(GenerateRandomPoints(envelope, count)));
        }

        public static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox envelope, int count = 25, int? randomSeed = null)
        {
            if (randomSeed != null) _random = new Random(randomSeed.Value);

            var result = new List<IGeometry>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new Point(
                    _random.NextDouble() * envelope.Width + envelope.Left,
                    _random.NextDouble() * envelope.Height + envelope.Bottom));
            }

            return result;
        }

        private static Features CreateFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new Features();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                // Calc radius for point
                // Be carefull, this only works, because OSM use spherical mercator projection in this case.
                // So we could use meters (200000) in example.

                // Get current position
                var position = Projection.SphericalMercator.ToLonLat(((Point)point).X, ((Point)point).Y);

                // Calc ground resolution in meters. One pixel of viewport for this latitude
                double groundResolution = Math.Cos(position.Y / 180.0 * Math.PI);

                // Now we can calc the radius of circle
                var radius = 200000 / groundResolution;

                var circle = new Circle(((Point)point).X, ((Point)point).Y, radius);
                features.Add(new Feature { Geometry = circle, ["Label"] = counter++.ToString() });
            }
            return features;
        }

        public static ILayer CreateRandomPointLayer(BoundingBox envelope, int count = 25, IStyle style = null)
        {
            return new Layer
            {
                DataSource = new MemoryProvider(GenerateRandomPoints(envelope, count)),
                Style = style ?? new VectorStyle { Fill = new Brush(Color.White), Outline = new Pen { Color = Color.Black, PenStyle = PenStyle.Dash } }
            };
        }
    }
}