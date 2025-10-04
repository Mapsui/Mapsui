using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Mapsui.Utilities;

namespace Mapsui.Layers;

/// <summary>
/// Represents a memory-based layer that exposes its data as an observable collection and synchronizes feature changes
/// with the underlying feature set.
/// </summary>
/// <remarks>This class enables two-way synchronization between an ObservableCollection of items and the set of
/// features exposed by the layer. Changes to the collection are automatically reflected in the feature set, and vice
/// versa. This is useful for scenarios where UI or other components need to observe and react to changes in the layer's
/// data in real time.</remarks>
/// <typeparam name="T">The type of items contained in the observable collection. Must be a reference type.</typeparam>
public class ObservableMemoryLayer<T> : MemoryLayer
    where T : class
{
    private ObservableCollection<T>? _observableCollection;
    private readonly ConcurrentHashSet<IFeature> _shadowCollection = new();
    private readonly Func<T, IFeature?> _getFeature;

    /// <summary>
    /// Initializes a new instance of the ObservableMemoryLayer class with the specified feature selector and optional
    /// name.
    /// </summary>
    /// <param name="getFeature">A function gets the IFeature instance related to the item of type T. This function is used to map items in the
    /// layer to their corresponding features. Cannot be null.</param>
    /// <param name="name">The optional name to assign to the layer. If null, a default name based on the type is used.</param>
    public ObservableMemoryLayer(Func<T, IFeature?> getFeature, string? name = null) : base(
        name ?? nameof(ObservableMemoryLayer<T>))
    {
        _getFeature = getFeature;
        base.Features = _shadowCollection;
    }

    /// <summary>
    /// Hide set from Base Features Collection because, if this is set than observable memory layer does not work
    /// </summary>
    public new IEnumerable<IFeature> Features => _shadowCollection;

    /// <summary>
    /// Gets or sets the underlying collection of items to observe for changes.
    /// </summary>
    /// <remarks>Assigning a new collection will update the internal state to reflect the contents of the
    /// provided collection and subscribe to its change notifications. If the collection is replaced, any previous event
    /// subscriptions are removed. Setting this property to null will clear the internal state and unsubscribe from
    /// change notifications.</remarks>
    public ObservableCollection<T>? ObservableCollection
    {
        get => _observableCollection;
        set
        {
            // safety check
            if (base.Features != _shadowCollection)
                base.Features = _shadowCollection;

            if (_observableCollection != null)
            {
                _observableCollection.CollectionChanged -= DataSource_CollectionChanged;
            }

            _observableCollection = value;
            if (_observableCollection != null)
            {
                _observableCollection.CollectionChanged += DataSource_CollectionChanged;
                _shadowCollection.Clear();
                foreach (var it in _observableCollection.ToArray()) // collection has been changed.
                {
                    var feature = _getFeature(it);
                    if (feature != null)
                        _shadowCollection.Add(feature);
                }
            }
            FeaturesWereModified();
            DataHasChanged();
        }
    }

    private void DataSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                {
                    foreach (var it in e.OldItems)
                    {
                        var feature = _getFeature((T)it);
                        if (feature != null)
                            _shadowCollection.TryRemove(feature);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        var feature = _getFeature((T)it);
                        if (feature != null)
                            _shadowCollection.Add(feature);
                    }
                }

                DataHasChanged();
                break;
            case NotifyCollectionChangedAction.Reset:
                _shadowCollection.Clear();
                if (_observableCollection != null)
                    foreach (var it in _observableCollection)
                    {
                        var feature = _getFeature(it);
                        if (feature != null)
                            _shadowCollection.Add(feature);
                    }

                DataHasChanged();
                break;
            case NotifyCollectionChangedAction.Move:
                // do nothing
                break;
        }
        FeaturesWereModified();
    }
}
