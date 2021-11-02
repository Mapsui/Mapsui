using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Styles;

namespace Mapsui.Providers
{
    public abstract class BaseFeature
    {
        private readonly Dictionary<string, object?> _dictionary = new();

        public BaseFeature()
        {
            Styles = new Collection<IStyle>();
        }

        public ICollection<IStyle> Styles { get; set; }

        public virtual object? this[string key]
        {
            get => _dictionary.ContainsKey(key) ? _dictionary[key] : null;
            set => _dictionary[key] = value;
        }

        public IEnumerable<string> Fields => _dictionary.Keys;
    }
}
