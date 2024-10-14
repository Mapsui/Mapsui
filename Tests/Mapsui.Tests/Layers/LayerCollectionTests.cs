using System.Linq;
using Mapsui.Layers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class LayerCollectionTests
{
    [Test]
    public void CopyToWithNormalConditions()
    {
        // arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer();
        using var layer2 = new MemoryLayer();
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        var size = layerCollection.Count();
        var array = new ILayer[size];

        // act
        layerCollection.CopyTo(array, 0);

        // assert
        ClassicAssert.AreEqual(2, array.Length);
        ClassicAssert.NotNull(array[0]);
        ClassicAssert.NotNull(array[1]);
    }

    [Test]
    public void CopyToAfterRemovingItem()
    {
        // arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer();
        using var layer2 = new MemoryLayer();
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        var size = layerCollection.Count();
        var array = new ILayer[size];
        layerCollection.Remove(layer1);

        // act
        layerCollection.CopyTo(array, 0);

        // assert
        ClassicAssert.AreEqual(2, array.Length);
        ClassicAssert.NotNull(array[0], "first element not null");
        // We have no crash but the seconds element is null.
        // This might have unpleasant consequences.
        ClassicAssert.Null(array[1], "second element IS null");
    }

    [Test]
    public void InsertWithNormalConditions()
    {
        // arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        // act
        layerCollection.Insert(1, layer3);

        // assert
        var list = layerCollection.ToList();
        ClassicAssert.AreEqual(3, list.Count);
        ClassicAssert.NotNull(list[0]);
        ClassicAssert.AreEqual("Layer1", list[0].Name);
        ClassicAssert.NotNull(list[1]);
        ClassicAssert.AreEqual("Layer3", list[1].Name);
        ClassicAssert.NotNull(list[2]);
        ClassicAssert.AreEqual("Layer2", list[2].Name);
    }

    [Test]
    public void InsertAfterRemoving()
    {
        // arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        layerCollection.Remove(layer1);

        // act
        layerCollection.Insert(1, layer3);

        // assert
        var list = layerCollection.ToList();
        ClassicAssert.AreEqual(2, list.Count);
        ClassicAssert.NotNull(list[0]);
        ClassicAssert.AreEqual("Layer2", list[0].Name);
        ClassicAssert.NotNull(list[1]);
        ClassicAssert.AreEqual("Layer3", list[1].Name);
    }

    [Test]
    public void MoveWithNormalConditions()
    {
        // arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);
        layerCollection.Add(layer3);

        // act
        layerCollection.Move(1, layer3);

        // assert
        var list = layerCollection.ToList();
        ClassicAssert.AreEqual(3, list.Count);
        ClassicAssert.NotNull(list[0]);
        ClassicAssert.AreEqual("Layer1", list[0].Name);
        ClassicAssert.NotNull(list[1]);
        ClassicAssert.AreEqual("Layer3", list[1].Name);
        ClassicAssert.NotNull(list[2]);
        ClassicAssert.AreEqual("Layer2", list[2].Name);
    }

    [Test]
    public void MoveAfterIndex()
    {
        // arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);
        layerCollection.Add(layer3);

        // act
        layerCollection.Move(3, layer1);

        // assert
        var list = layerCollection.ToList();
        ClassicAssert.AreEqual(3, list.Count);
        ClassicAssert.NotNull(list[0]);
        ClassicAssert.AreEqual("Layer2", list[0].Name);
        ClassicAssert.NotNull(list[1]);
        ClassicAssert.AreEqual("Layer3", list[1].Name);
        ClassicAssert.NotNull(list[2]);
        ClassicAssert.AreEqual("Layer1", list[2].Name);
    }

    [Test]
    public void AddToGroup()
    {
        // Arrange
        var layerCollection = new LayerCollection();

        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        using var layer4 = new MemoryLayer() { Name = "Layer4" };
        using var layer5 = new MemoryLayer() { Name = "Layer5" };
        using var layer6 = new MemoryLayer() { Name = "Layer6" };

        var groupOnTop = -1;
        var groupOnMiddle = 0;
        var groupOnBottom = 1;

        // Act
        layerCollection.Add(layer1, groupOnTop);
        layerCollection.Add(layer2, groupOnMiddle);
        layerCollection.Add(layer3, groupOnBottom);
        layerCollection.Add(layer4, groupOnTop);
        layerCollection.Add(layer5, groupOnMiddle);
        layerCollection.Add(layer6, groupOnBottom);

        // Assert
        var list = layerCollection.ToList();
        Assert.That(list.Count, Is.EqualTo(6));

        Assert.That(list[0].Name, Is.EqualTo("Layer1"));
        Assert.That(list[1].Name, Is.EqualTo("Layer4"));
        Assert.That(list[2].Name, Is.EqualTo("Layer2"));
        Assert.That(list[3].Name, Is.EqualTo("Layer5"));
        Assert.That(list[4].Name, Is.EqualTo("Layer3"));
        Assert.That(list[5].Name, Is.EqualTo("Layer6"));
    }

    [Test]
    public void InsertToGroup()
    {
        // Arrange
        var layerCollection = new LayerCollection();

        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        using var layer4 = new MemoryLayer() { Name = "Layer4" };
        using var layer5 = new MemoryLayer() { Name = "Layer5" };
        using var layer6 = new MemoryLayer() { Name = "Layer6" };

        var groupOnTop = -1;
        var groupOnMiddle = 0;
        var groupOnBottom = 1;

        // Act
        layerCollection.Insert(0, layer1, groupOnTop);
        layerCollection.Insert(0, layer2, groupOnMiddle);
        layerCollection.Insert(0, layer3, groupOnBottom);
        layerCollection.Insert(1, layer4, groupOnTop);
        layerCollection.Insert(1, layer5, groupOnMiddle);
        layerCollection.Insert(1, layer6, groupOnBottom);

        // Assert
        var list = layerCollection.ToList();
        Assert.That(list.Count, Is.EqualTo(6));

        Assert.That(list[0].Name, Is.EqualTo("Layer1"));
        Assert.That(list[1].Name, Is.EqualTo("Layer4"));
        Assert.That(list[2].Name, Is.EqualTo("Layer2"));
        Assert.That(list[3].Name, Is.EqualTo("Layer5"));
        Assert.That(list[4].Name, Is.EqualTo("Layer3"));
        Assert.That(list[5].Name, Is.EqualTo("Layer6"));
    }
}
