using System;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projections;

namespace Mapsui.GeometryLayer
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

        public GeometryFeature(IGeometry geometry)
        {
            Geometry = geometry;
        }

        public IGeometry? Geometry { get; set; }

        public MRect? Extent => Geometry?.BoundingBox.ToMRect(); // Todo: Make not-nullable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GeometryFeature()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                foreach (var keyValuePair in RenderedGeometry)
                {
                    var disposable = keyValuePair.Value as IDisposable;
                    disposable?.Dispose();
                }
                RenderedGeometry.Clear();
            }
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
