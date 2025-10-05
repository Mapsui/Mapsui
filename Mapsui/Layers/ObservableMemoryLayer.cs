using System;
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
    private readonly ConcurrentHashSet<ShadowItem<T>> _shadowCollection = new();
    private readonly Func<T, IFeature?> _itemToFeature;

    /// <summary>
    /// Initializes a new instance of the ObservableMemoryLayer class with the specified feature selector and optional
    /// name.
    /// </summary>
    /// <param name="itemToFeature">A function gets the IFeature instance related to the item of type T. This function is used to map items in the
    /// layer to their corresponding features. Cannot be null.</param>
    /// <param name="name">The optional name to assign to the layer. If null, a default name based on the type is used.</param>
    public ObservableMemoryLayer(Func<T, IFeature?> itemToFeature, string? name = null) : base(name ?? nameof(ObservableMemoryLayer<T>))
    {
        _itemToFeature = itemToFeature;
        Features = _shadowCollection.Select(i => i.Feature);
    }

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
                    var feature = _itemToFeature(it);
                    if (feature != null)
                    {
                        _ = _shadowCollection.Add(new ShadowItem<T>(it, feature));
                    }
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
                        var shadowItem = _shadowCollection.FirstOrDefault(i => i.Item == it);
                        if (shadowItem != null)
                            _ = _shadowCollection.TryRemove(shadowItem);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        var feature = _itemToFeature((T)it);
                        if (feature != null)
                        {
                            var shadowItem = new ShadowItem<T>((T)it, feature);
                            _shadowCollection.Add(shadowItem);
                        }
                    }
                }

                FeaturesWereModified();
                DataHasChanged();
                break;
            case NotifyCollectionChangedAction.Reset:
                _shadowCollection.Clear();
                if (_observableCollection != null)
                    foreach (var it in _observableCollection)
                    {
                        var feature = _itemToFeature(it);
                        if (feature != null)
                        {
                            var shadowItem = new ShadowItem<T>(it, feature);
                            _ = _shadowCollection.Add(shadowItem);
                        }
                    }

                FeaturesWereModified();
                DataHasChanged();
                break;
            case NotifyCollectionChangedAction.Move:
                // do nothing
                break;
        }
    }

    private class ShadowItem<U>(U item, IFeature feature)
    {
        public U Item { get; } = item;
        public IFeature Feature { get; } = feature;
    }
}
