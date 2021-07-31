using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class MutatingTriangleSample : ISample
    {
        public string Name => "Mutating triangle";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        private static readonly Random Random = new Random(0);

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateMutatingTriangleLayer(map.Envelope));
            return map;
        }

        private static ILayer CreateMutatingTriangleLayer(BoundingBox envelope)
        {   
            var layer = new MemoryLayer();
           
            var polygon = new Polygon(new LinearRing(GenerateRandomPoints(envelope, 3)));
            var feature = new Feature() { Geometry = polygon };
            var features = new Features();
            features.Add(feature);

            layer.DataSource = new MemoryProvider(features);

            PeriodicTask.Run(() =>
            {
                polygon.ExteriorRing = new LinearRing(GenerateRandomPoints(envelope, 3));
                // Clear cache for change to show
                feature.RenderedGeometry.Clear();
                // Trigger DataChanged notification
                layer.DataHasChanged();
            },
            TimeSpan.FromMilliseconds(1000));

            return layer;
        }

        public static IEnumerable<Point> GenerateRandomPoints(BoundingBox envelope, int count = 25)
        {
            var result = new List<Point>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new Point(
                    Random.NextDouble() * envelope.Width + envelope.Left,
                    Random.NextDouble() * envelope.Height + envelope.Bottom));
            }

            result.Add(result[0]); // close polygon by adding start point.

            return result;
        }

        public class PeriodicTask
        {
            public static async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(period, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                        action();
                }
            }

            public static Task Run(Action action, TimeSpan period)
            {
                return Run(action, period, CancellationToken.None);
            }
        }
    }
}
