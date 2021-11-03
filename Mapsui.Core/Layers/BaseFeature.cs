using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    public abstract class BaseFeature : IFeature
    {
        private readonly Dictionary<string, object?> _dictionary = new();

        public ICollection<IStyle> Styles { get; set; } = new Collection<IStyle>();
        public IEnumerable<string> Fields => _dictionary.Keys;
        public MRect? BoundingBox { get; }

        public virtual object? this[string key]
        {
            get => _dictionary.ContainsKey(key) ? _dictionary[key] : null;
            set => _dictionary[key] = value;
        }

    }
}
