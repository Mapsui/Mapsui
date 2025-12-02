using System.Linq;
using Mapsui;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using NUnit.Framework;

namespace QuickStart.Avalonia.Tests;

/// <summary>
/// Basic non-UI tests to verify Mapsui core functionality works correctly.
/// These tests validate the same components used in the QuickStart sample.
/// </summary>
[TestFixture]
public class MapsuiBasicTests
{
    [Test]
    public void Map_CanBeCreated()
    {
        // Arrange & Act
        using var map = new Map();

        // Assert
        Assert.That(map, Is.Not.Null);
        Assert.That(map.Layers, Is.Not.Null);
        Assert.That(map.Layers.Count, Is.EqualTo(0));
    }

    [Test]
    public void Map_CanAddOpenStreetMapLayer()
    {
        // Arrange
        using var map = new Map();
        using var osmLayer = OpenStreetMap.CreateTileLayer();

        // Act
        map.Layers.Add(osmLayer);

        // Assert
        Assert.That(map.Layers.Count, Is.EqualTo(1));
        Assert.That(map.Layers.GetLayersOfAllGroups().First(), Is.EqualTo(osmLayer));
    }

    [Test]
    public void OpenStreetMapLayer_HasCorrectProperties()
    {
        // Arrange & Act
        using var osmLayer = OpenStreetMap.CreateTileLayer();

        // Assert
        Assert.That(osmLayer, Is.Not.Null);
        Assert.That(osmLayer, Is.InstanceOf<TileLayer>());
        Assert.That(osmLayer.Name, Is.EqualTo("OpenStreetMap"));
    }

    [Test]
    public void Map_NavigatorIsInitialized()
    {
        // Arrange & Act
        using var map = new Map();

        // Assert
        Assert.That(map.Navigator, Is.Not.Null);
    }

    [Test]
    public void Map_CanClearLayers()
    {
        // Arrange
        using var map = new Map();
        using var osmLayer = OpenStreetMap.CreateTileLayer();
        map.Layers.Add(osmLayer);

        // Act
        map.Layers.Clear();

        // Assert
        Assert.That(map.Layers.Count, Is.EqualTo(0));
    }
}
