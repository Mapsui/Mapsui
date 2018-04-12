using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Providers
{
    public class Feature : IFeature, IDisposable
    {
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();
        private bool _disposed;

        public Feature()
        {
            RenderedGeometry = new Dictionary<IStyle, object>();
            Styles = new Collection<IStyle>();
        }

        public Feature(IFeature feature)
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

        public IGeometry Geometry { get; set; }

        public IDictionary<IStyle, object> RenderedGeometry { get; private set; }

        public ICollection<IStyle> Styles { get; set; }

        public virtual object this[string key]
        {
            get { return _dictionary.ContainsKey(key) ? _dictionary[key] : null; }
            set { _dictionary[key] = value; }
        }

        public IEnumerable<string> Fields
        {
            get { return _dictionary.Keys; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Feature()
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
    }
}
