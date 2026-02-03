using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Mapsui.Experimental.Layers;
using Mapsui.Layers;
using NUnit.Framework;

#pragma warning disable IDISP001 // Dispose Disposable

namespace Mapsui.Tests.Layers;

[TestFixture]
public class ObservableCollectionLayerTests
{
    [Test]
    public void ObservableCollectionLayer_WithObservableCollection_ShouldPopulateFeatures()
    {
        // Arrange
        var collection = new ObservableCollection<TestItem>
        {
            new TestItem { Id = 1, X = 0, Y = 0 },
            new TestItem { Id = 2, X = 10, Y = 10 }
        };

        // Act
        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection
        };

        // Assert
        var features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(2));
    }

    [Test]
    public void ObservableCollectionLayer_AddItem_ShouldAddFeature()
    {
        // Arrange
        var collection = new ObservableCollection<TestItem>
        {
            new TestItem { Id = 1, X = 0, Y = 0 }
        };

        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection
        };

        // Act
        collection.Add(new TestItem { Id = 2, X = 10, Y = 10 });

        // Assert
        var features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(2));
    }

    [Test]
    public void ObservableCollectionLayer_RemoveItem_ShouldRemoveFeature()
    {
        // Arrange
        var item1 = new TestItem { Id = 1, X = 0, Y = 0 };
        var item2 = new TestItem { Id = 2, X = 10, Y = 10 };
        var collection = new ObservableCollection<TestItem> { item1, item2 };

        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection
        };

        // Act
        collection.Remove(item1);

        // Assert
        var features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(1));
    }

    [Test]
    public void ObservableCollectionLayer_ReplaceItem_ShouldUpdateFeature()
    {
        // Arrange
        var collection = new ObservableCollection<TestItem>
        {
            new TestItem { Id = 1, X = 0, Y = 0 }
        };

        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection
        };

        // Act
        collection[0] = new TestItem { Id = 2, X = 10, Y = 10 };

        // Assert
        var features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(1));
    }

    [Test]
    public void ObservableCollectionLayer_ClearCollection_ShouldClearFeatures()
    {
        // Arrange
        var collection = new ObservableCollection<TestItem>
        {
            new TestItem { Id = 1, X = 0, Y = 0 },
            new TestItem { Id = 2, X = 10, Y = 10 }
        };

        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection
        };

        // Act
        collection.Clear();

        // Assert
        Assert.That(layer.Extent, Is.Null);
    }

    [Test]
    public void ObservableCollectionLayer_WithCustomINotifyCollectionChanged_ShouldWork()
    {
        // Arrange
        var collection = new CustomObservableCollection<TestItem>
        {
            new TestItem { Id = 1, X = 0, Y = 0 }
        };

        // Act
        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection
        };

        // Assert - should have 1 feature
        var features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(1));

        // Act - add item
        collection.Add(new TestItem { Id = 2, X = 10, Y = 10 });

        // Assert - should have 2 features
        features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(2));
    }

    [Test]
    public void ObservableCollectionLayer_SetToNull_ShouldClearFeatures()
    {
        // Arrange
        var collection = new ObservableCollection<TestItem>
        {
            new TestItem { Id = 1, X = 0, Y = 0 }
        };

        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection
        };

        // Act
        layer.ObservableCollection = null;

        // Assert
        var features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(0));
    }

    [Test]
    public void ObservableCollectionLayer_ReplaceCollection_ShouldUnsubscribeFromOld()
    {
        // Arrange
        var collection1 = new ObservableCollection<TestItem>
        {
            new TestItem { Id = 1, X = 0, Y = 0 }
        };
        var collection2 = new ObservableCollection<TestItem>
        {
            new TestItem { Id = 2, X = 10, Y = 10 },
            new TestItem { Id = 3, X = 20, Y = 20 }
        };

        using var layer = new ObservableCollectionLayer<TestItem>(item => new PointFeature(item.X, item.Y))
        {
            ObservableCollection = collection1
        };

        // Act
        layer.ObservableCollection = collection2;
        collection1.Add(new TestItem { Id = 4, X = 30, Y = 30 }); // Should not affect layer

        // Assert
        var features = layer.GetFeatures(layer.Extent, 1).ToList();
        Assert.That(features.Count, Is.EqualTo(2)); // Only items from collection2
    }

    private class TestItem
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    // Custom implementation of INotifyCollectionChanged to test the flexibility
    private class CustomObservableCollection<T> : List<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public new void Add(T item)
        {
            base.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public new void Clear()
        {
            base.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
