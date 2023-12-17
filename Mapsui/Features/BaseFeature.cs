using Mapsui.Styles;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Mapsui.Layers;

public abstract class BaseFeature
{
    // last used feature id
    private static long _currentFeatureId;

    protected BaseFeature(long id)
    {
        Id = id;
    }

    protected BaseFeature()
    {
        Id = NextId();
    }

    private static long NextId()
    {
        return Interlocked.Increment(ref _currentFeatureId);
    }

    public long Id { get; private set; }

    protected BaseFeature(BaseFeature baseFeature) : this()
    {
        Copy(baseFeature);
    }

    protected BaseFeature(BaseFeature baseFeature, long id) : this(id)
    {
        Copy(baseFeature);
    }

    private void Copy(BaseFeature baseFeature)
    {
        Styles = baseFeature.Styles.ToList();
        foreach (var field in baseFeature.Fields)
            this[field] = baseFeature[field];
    }

    private readonly Dictionary<string, object?> _dictionary = [];

    public ICollection<IStyle> Styles { get; set; } = new Collection<IStyle>();
    public IEnumerable<string> Fields => _dictionary.Keys;

    public virtual object? this[string key]
    {
        get => _dictionary.TryGetValue(key, out var value) ? value : null;
        set => _dictionary[key] = value;
    }

    public void Modified()
    {
        // is modified needs a new id.
        Id = NextId();
    }
}
