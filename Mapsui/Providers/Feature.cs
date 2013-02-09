using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Providers
{
    public class Feature : IFeature
    {
        private readonly Dictionary<string, object> _dictionary;

        public Feature()
        {
            _dictionary = new Dictionary<string, object>();
            RenderedGeometry = new Dictionary<IStyle, object>();
            Styles = new Collection<IStyle>();
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
    }

}
