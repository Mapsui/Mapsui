using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Providers
{
    public class GeometryFeature : BaseFeature, IGeometryFeature, IDisposable
    {
        private bool _disposed;

        public GeometryFeature()
        {
            RenderedGeometry = new Dictionary<IStyle, object>();
            Styles = new Collection<IStyle>();
        }

        public GeometryFeature(IGeometryFeature feature)
        {
            Geometry = feature.Geometry;
            RenderedGeometry = feature.RenderedGeometry.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            Styles = feature.Styles.ToList();
            foreach (var field in feature.Fields)
            {
                this[field] = feature[field];
            }
        }

        public IGeometry? Geometry { get; set; }

        public IDictionary<IStyle, object> RenderedGeometry { get; }

        public MRect? BoundingBox => Geometry.BoundingBox.ToMRect();

        ~GeometryFeature()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
    }
}
