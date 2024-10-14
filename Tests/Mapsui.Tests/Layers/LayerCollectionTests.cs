using System.Linq;
using Mapsui.Layers;
using NUnit.Framework;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class LayerCollectionTests
{
    [Test]
    public void InsertWithNormalConditions()
    {
        // Arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);

        // Act
        layerCollection.Insert(1, layer3);

        // Assert
        var list = layerCollection.ToList();
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.Not.Null);
        Assert.That(list[0].Name, Is.EqualTo("Layer1"));
        Assert.That(list[1], Is.Not.Null);
        Assert.That(list[1].Name, Is.EqualTo("Layer3"));
        Assert.That(list[2], Is.Not.Null);
        Assert.That(list[2].Name, Is.EqualTo("Layer2"));
    }

    [Test]
    public void InsertAfterRemoving()
    {
        // Arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        using var layer1Group2 = new MemoryLayer() { Name = "Layer1Group2" };
        using var layer2Group2 = new MemoryLayer() { Name = "Layer2Group2" };

        layerCollection.Add(layer1Group2, 1);
        layerCollection.Add(layer1);
        layerCollection.Add(layer2Group2, 1);
        layerCollection.Add(layer2);

        layerCollection.Remove(layer1);

        // Act
        layerCollection.Insert(1, layer3);

        // Assert
        var list = layerCollection.GetLayersOfGroup(0);
        Assert.That(list.Length, Is.EqualTo(2));
        Assert.That(list[0], Is.Not.Null);
        Assert.That(list[0].Name, Is.EqualTo("Layer2"));
        Assert.That(list[1], Is.Not.Null);
        Assert.That(list[1].Name, Is.EqualTo("Layer3"));
    }

    [Test]
    public void MoveWithNormalConditions()
    {
        // Arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);
        layerCollection.Add(layer3);

        // Act
        layerCollection.Move(1, layer3);

        // Assert
        var list = layerCollection.ToList();
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.Not.Null);
        Assert.That(list[0].Name, Is.EqualTo("Layer1"));
        Assert.That(list[1], Is.Not.Null);
        Assert.That(list[1].Name, Is.EqualTo("Layer3"));
        Assert.That(list[2], Is.Not.Null);
        Assert.That(list[2].Name, Is.EqualTo("Layer2"));
    }

    [Test]
    public void MoveAfterIndex()
    {
        // Arrange
        var layerCollection = new LayerCollection();
        using var layer1 = new MemoryLayer() { Name = "Layer1" };
        using var layer2 = new MemoryLayer() { Name = "Layer2" };
        using var layer3 = new MemoryLayer() { Name = "Layer3" };
        layerCollection.Add(layer1);
        layerCollection.Add(layer2);
        layerCollection.Add(layer3);

        // Act
        layerCollection.Move(3, layer1);

        // Assert
        var list = layerCollection.ToList();
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0].Name, Is.EqualTo("Layer2"));
        Assert.That(list[1].Name, Is.EqualTo("Layer3"));
        Assert.That(list[2].Name, Is.EqualTo("Layer1"));
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
