using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mapsui.Layers;

public abstract class BaseFeature : IFeature
{
    private static long _currentFeatureId; // last used feature id
    private readonly Dictionary<string, object?> _dictionary = [];

    protected BaseFeature(long id)
    {
        Id = id;
    }

    protected BaseFeature()
    {
        Id = NextId();
    }

    protected BaseFeature(BaseFeature baseFeature) : this()
    {
        Copy(baseFeature);
    }

    protected BaseFeature(BaseFeature baseFeature, long id) : this(id)
    {
        Copy(baseFeature);
    }

    /// <inheritdoc />
    public long Id { get; private set; }

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

    private static long NextId()
    {
        return Interlocked.Increment(ref _currentFeatureId);
    }

    private void Copy(BaseFeature baseFeature)
    {
        Id = baseFeature.Id; // Copy the Id to maintain the same identity. If one of the copies is updated Modified() needs to be called.
        Styles = baseFeature.Styles.ToList(); // Styles is an ICollection, we need ToList instead of ToArray to prevent a NotSupportedException on Styles.Clear();
        foreach (var field in baseFeature.Fields)
            this[field] = baseFeature[field];
        Data = baseFeature.Data;
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
