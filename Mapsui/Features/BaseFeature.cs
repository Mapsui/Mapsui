using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mapsui.Styles;

namespace Mapsui.Layers;

public abstract class BaseFeature : IDisposable
{
    public BaseFeature() { }

    public BaseFeature(BaseFeature baseFeature)
    {
        Styles = baseFeature.Styles.ToList();
        foreach (var field in baseFeature.Fields)
            this[field] = baseFeature[field];
    }

    private readonly Dictionary<string, object?> _dictionary = new();

    public ICollection<IStyle> Styles { get; set; } = new Collection<IStyle>();
    public IEnumerable<string> Fields => _dictionary.Keys;

    public virtual object? this[string key]
    {
        get => _dictionary.ContainsKey(key) ? _dictionary[key] : null;
        set => _dictionary[key] = value;
    }

    public IDictionary<IStyle, object> RenderedGeometry { get; set; } = new Dictionary<IStyle, object>();

    public virtual void Dispose()
    {
    }
}
