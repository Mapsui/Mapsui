using System.Linq;
using Mapsui.Layers;
using NUnit.Framework;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class LayerCollectionTests
{
    [Test]
    public void CopyToWithNormalConditions()
    {
        // arrange
        var layerCollection = new LayerCollection();
        var layer1 = new MemoryLayer();
        var layer2 = new MemoryLayer();
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        var size = layerCollection.Count();
        var array = new ILayer[size];

        // act
        layerCollection.CopyTo(array, 0);

        // assert
        Assert.AreEqual(2, array.Length);
        Assert.NotNull(array[0]);
        Assert.NotNull(array[1]);
    }

    [Test]
    public void CopyToAfterRemovingItem()
    {
        // arrange
        var layerCollection = new LayerCollection();
        var layer1 = new MemoryLayer();
        var layer2 = new MemoryLayer();
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        var size = layerCollection.Count();
        var array = new ILayer[size];
        layerCollection.Remove(layer1);

        // act
        layerCollection.CopyTo(array, 0);

        // assert
        Assert.AreEqual(2, array.Length);
        Assert.NotNull(array[0], "first element not null");
        // We have no crash but the seconds element is null.
        // This might have unpleasant consequences.
        Assert.Null(array[1], "second element IS null");
    }

    [Test]
    public void InsertWithNormalConditions()
    {
        // arrange
        var layerCollection = new LayerCollection();
        var layer1 = new MemoryLayer() { Name = "Layer1" };
        var layer2 = new MemoryLayer() { Name = "Layer2" };
        var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        // act
        layerCollection.Insert(1, layer3);

        // assert
        var list = layerCollection.ToList();
        Assert.AreEqual(3, list.Count);
        Assert.NotNull(list[0]);
        Assert.AreEqual("Layer1", list[0].Name);
        Assert.NotNull(list[1]);
        Assert.AreEqual("Layer3", list[1].Name);
        Assert.NotNull(list[2]);
        Assert.AreEqual("Layer2", list[2].Name);
    }

    [Test]
    public void InsertAfterRemoving()
    {
        // arrange
        var layerCollection = new LayerCollection();
        var layer1 = new MemoryLayer() { Name = "Layer1" };
        var layer2 = new MemoryLayer() { Name = "Layer2" };
        var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        layerCollection.Remove(layer1);

        // act
        layerCollection.Insert(1, layer3);

        // assert
        var list = layerCollection.ToList();
        Assert.AreEqual(2, list.Count);
        Assert.NotNull(list[0]);
        Assert.AreEqual("Layer2", list[0].Name);
        Assert.NotNull(list[1]);
        Assert.AreEqual("Layer3", list[1].Name);
    }

    [Test]
    public void MoveWithNormalConditions()
    {
        // arrange
        var layerCollection = new LayerCollection();
        var layer1 = new MemoryLayer() { Name = "Layer1" };
        var layer2 = new MemoryLayer() { Name = "Layer2" };
        var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);
        layerCollection.Add(layer3);

        // act
        layerCollection.Move(1, layer3);

        // assert
        var list = layerCollection.ToList();
        Assert.AreEqual(3, list.Count);
        Assert.NotNull(list[0]);
        Assert.AreEqual("Layer1", list[0].Name);
        Assert.NotNull(list[1]);
        Assert.AreEqual("Layer3", list[1].Name);
        Assert.NotNull(list[2]);
        Assert.AreEqual("Layer2", list[2].Name);
    }

    [Test]
    public void MoveAfterIndex()
    {
        // arrange
        var layerCollection = new LayerCollection();
        var layer1 = new MemoryLayer() { Name = "Layer1" };
        var layer2 = new MemoryLayer() { Name = "Layer2" };
        var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);
        layerCollection.Add(layer3);

        // act
        layerCollection.Move(3, layer1);

        // assert
        var list = layerCollection.ToList();
        Assert.AreEqual(3, list.Count);
        Assert.NotNull(list[0]);
        Assert.AreEqual("Layer2", list[0].Name);
        Assert.NotNull(list[1]);
        Assert.AreEqual("Layer3", list[1].Name);
        Assert.NotNull(list[2]);
        Assert.AreEqual("Layer1", list[2].Name);
    }
}
