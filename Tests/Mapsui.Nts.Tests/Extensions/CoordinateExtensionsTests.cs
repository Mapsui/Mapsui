using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Mapsui.Nts.Extensions;
using System;

namespace Mapsui.Nts.Tests;

[TestFixture]
public class CoordinateExtensionsTests
{
    [Test]
    public void ToLineStringShouldNotThrowWhenCoordinatesIsEmpty()
    {
        // Arrange
        IEnumerable<Coordinate> coordinates = new List<Coordinate>();

        // Act & Assert
        Assert.DoesNotThrow(() => coordinates.ToLineString());
    }
}

