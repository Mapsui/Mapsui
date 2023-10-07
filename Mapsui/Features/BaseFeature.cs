using System;
using System.Collections.Concurrent;
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
    private IDictionary<IStyle, object>? _renderedGeometry;

    public ICollection<IStyle> Styles { get; set; } = new Collection<IStyle>();
    public IEnumerable<string> Fields => _dictionary.Keys;

    public virtual object? this[string key]
    {
        get => _dictionary.ContainsKey(key) ? _dictionary[key] : null;
        set => _dictionary[key] = value;
    }

    public IDictionary<IStyle, object> RenderedGeometry
    {
        get => _renderedGeometry ??= new ConcurrentDictionary<IStyle, object>();
        set => _renderedGeometry = value;
    }

    public void Modified()
    {
        ClearRenderedGeometry();
    }

    public virtual void Dispose()
    {
        ClearRenderedGeometry();
    }

    public void ClearRenderedGeometry()
    {
        if (_renderedGeometry != null)
        {
            var values = _renderedGeometry.Values.ToArray();
            _renderedGeometry = null;
            foreach (var value in values)
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
