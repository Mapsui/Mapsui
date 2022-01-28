using System;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;

namespace Mapsui.GeometryLayers
{
    public class GeometryFeature : BaseFeature, IFeature, IDisposable
    {
        private bool _disposed;

        public GeometryFeature()
        {
        }

        public GeometryFeature(GeometryFeature geometryFeature) : base(geometryFeature)
        {
            Geometry = geometryFeature.Geometry?.Copy();
        }

        public GeometryFeature(Geometry geometry)
        {
            Geometry = geometry;
        }

        public Geometry? Geometry { get; set; }

        public MRect? Extent => Geometry?.BoundingBox().ToMRect(); // Todo: Make not-nullable

        public override void Dispose()
        {
            if (_disposed) return;
            base.Dispose();

            foreach (var keyValuePair in RenderedGeometry)
            {
                var disposable = keyValuePair.Value as IDisposable;
                disposable?.Dispose();
            }
            RenderedGeometry.Clear();

            _disposed = true;
        }

        public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
        {
            var vertices = Geometry.AllVertices();
            foreach (var vertex in vertices)
            {
                visit(vertex.X, vertex.Y, (x, y) => {
                    vertex.X = x;
                    vertex.Y = y;
                });
            }
        }
    }
}
