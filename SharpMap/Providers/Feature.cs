using System.Collections.Generic;
using SharpMap.Geometries;
using SharpMap.Styles;

namespace SharpMap.Providers
{
    public class Feature : IFeature
    {
        private readonly Dictionary<string, object> dictionary;

        public Feature()
        {
            dictionary = new Dictionary<string, object>();
        }

        public IGeometry Geometry { get; set; }

        public object RenderedGeometry { get; set; }

        public IStyle Style { get; set; }

        public virtual object this[string key]
        {
            get { return dictionary.ContainsKey(key) ? dictionary[key] : null; }
            set { dictionary[key] = value; }
        }

        public IEnumerable<string> Fields
        {
            get { foreach (var key in dictionary.Keys) yield return key; }
        }
    }

}
