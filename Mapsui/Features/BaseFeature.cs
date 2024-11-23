using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mapsui.Layers;

public abstract class BaseFeature : IFeature
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

    /// <inheritdoc />
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
        Styles = [.. baseFeature.Styles];
        foreach (var field in baseFeature.Fields)
            this[field] = baseFeature[field];
    }

    private readonly Dictionary<string, object?> _dictionary = [];

    /// <inheritdoc />
    public ICollection<IStyle> Styles { get; set; } = [];

    /// <inheritdoc />
    public IEnumerable<string> Fields => _dictionary.Keys;

    /// <inheritdoc />
    public abstract MRect? Extent { get; }

    /// <inheritdoc />
    public object? Data { get; set; }

    /// <inheritdoc />
    public virtual object? this[string key]
    {
        get => _dictionary.TryGetValue(key, out var value) ? value : null;
        set => _dictionary[key] = value;
    }

    /// <inheritdoc />
    virtual public void Modified()
    {
        // is modified needs a new id.
        Id = NextId();
    }

    /// <inheritdoc />
    public abstract void CoordinateVisitor(Action<double, double, CoordinateSetter> visit);

    /// <inheritdoc />
    public abstract object Clone();

}
