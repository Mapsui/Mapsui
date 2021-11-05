using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    public abstract class BaseFeature
    {
        private readonly Dictionary<string, object?> _dictionary = new();

        public ICollection<IStyle> Styles { get; set; } = new Collection<IStyle>();
        public IEnumerable<string> Fields => _dictionary.Keys;

        public virtual object? this[string key]
        {
            get => _dictionary.ContainsKey(key) ? _dictionary[key] : null;
            set => _dictionary[key] = value;
        }

    }
}
