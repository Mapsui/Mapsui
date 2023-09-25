using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Mapsui.Utilities;

namespace Mapsui.Layers;

public class ObservableMemoryLayer<T> : MemoryLayer
    where T : class
{
    private ObservableCollection<T>? _observableCollection;
    private readonly ConcurrentHashSet<IFeature> _shadowCollection;
    private readonly Func<T, IFeature?> _getFeature;

    public ObservableMemoryLayer(Func<T, IFeature?> getFeature, string? name = null) : base(
        name ?? nameof(ObservableMemoryLayer<T>))
    {
        _getFeature = getFeature;
        _shadowCollection = new ConcurrentHashSet<IFeature>();
        base.Features = _shadowCollection;
    }

    /// <summary>
    /// Hide set from Base Features Collection because, if this is set than observable memory layer does not work
    /// </summary>
    public new IEnumerable<IFeature> Features => _shadowCollection;

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
    }
}
