using System;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using NUnit.Framework;

namespace Mapsui.Tests.Extensions;

[TestFixture]
public class StyleExtensionsTests
{
    private static Mapsui.Viewport CreateViewport(double resolution = 1.0)
        => new Mapsui.Viewport(0, 0, resolution, 0, 256, 256);

    private static IFeature CreateFeature()
        => new PointFeature(new Mapsui.MPoint(0, 0));

    [Test]
    public void GetStylesToApply_NullStyle_ReturnsEmpty()
    {
        // Act
        var result = ((IStyle?)null).GetStylesToApply(CreateFeature(), CreateViewport());

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetStylesToApply_SimpleStyle_Visible_ReturnsStyle()
    {
        // Arrange
        var style = new VectorStyle { Enabled = true, MinVisible = 0, MaxVisible = double.MaxValue };

        // Act
        var result = style.GetStylesToApply(CreateFeature(), CreateViewport(10)).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.SameAs(style));
    }

    [Test]
    public void GetStylesToApply_SimpleStyle_NotEnabled_ReturnsEmpty()
    {
        // Arrange
        var style = new VectorStyle { Enabled = false };

        // Act
        var result = style.GetStylesToApply(CreateFeature(), CreateViewport()).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetStylesToApply_SimpleStyle_OutsideResolution_ReturnsEmpty()
    {
        // Arrange
        var tooLow = new VectorStyle { MinVisible = 2.0, MaxVisible = 100.0 }; // resolution 1 < MinVisible
        var tooHigh = new VectorStyle { MinVisible = 0.0, MaxVisible = 0.5 };  // resolution 1 > MaxVisible

        // Act
        var resultLow = tooLow.GetStylesToApply(CreateFeature(), CreateViewport(1.0));
        var resultHigh = tooHigh.GetStylesToApply(CreateFeature(), CreateViewport(1.0));

        // Assert
        Assert.That(resultLow, Is.Empty);
        Assert.That(resultHigh, Is.Empty);
    }

    [Test]
    public void GetStylesToApply_StyleCollection_FlattensAndFilters_OrderPreserved()
    {
        // Arrange
        var v1 = new VectorStyle { Enabled = true };
        var v2 = new VectorStyle { Enabled = false }; // filtered out
        var v3 = new VectorStyle { MinVisible = 5, MaxVisible = 10 }; // filtered out at resolution 1
        var v4 = new VectorStyle { Enabled = true };
        var v5 = new VectorStyle { Enabled = false }; // filtered out

        var nested = new StyleCollection();
        nested.Styles.Add(v4);
        nested.Styles.Add(v5);

        var root = new StyleCollection();
        root.Styles.Add(v1);
        root.Styles.Add(v2);
        root.Styles.Add(v3);
        root.Styles.Add(nested);

        // Act
        var result = root.GetStylesToApply(CreateFeature(), CreateViewport(1)).ToList();

        // Assert - depth-first order and filtering respected: v1, v4
        Assert.That(result, Is.EquivalentTo(new[] { v1, v4 }));
        Assert.That(result[0], Is.SameAs(v1));
        Assert.That(result[1], Is.SameAs(v4));
    }

    [Test]
    public void GetStylesToApply_ThemeStyle_ReturnsResolvedFlattened()
    {
        // Arrange: theme returns a collection with two styles, one disabled
        var enabled = new VectorStyle { Enabled = true };
        var disabled = new VectorStyle { Enabled = false };
        var theme = new ThemeStyle((f, v) => new StyleCollection
        {
            Styles =
            {
                enabled,
                disabled
            }
        });

        // Act
        var result = theme.GetStylesToApply(CreateFeature(), CreateViewport()).ToList();

        // Assert - only the enabled resolved style remains
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.SameAs(enabled));
    }

    [Test]
    public void GetStylesToApply_ThemeStyle_ReturnsNull_YieldsEmpty()
    {
        // Arrange
        var theme = new ThemeStyle((f, v) => null);

        // Act
        var result = theme.GetStylesToApply(CreateFeature(), CreateViewport());

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetStylesToApply_CycleInStyleCollection_Throws()
    {
        // Arrange
        var sc = new StyleCollection();
        sc.Styles.Add(sc); // create a direct cycle

        // Act + Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _ = sc.GetStylesToApply(CreateFeature(), CreateViewport()).ToList());
        Assert.That(ex!.Message, Does.Contain("Cycle detected in style graph"));
    }

    [Test]
    public void GetStylesToApply_CycleViaTheme_Throws()
    {
        // Arrange: theme returns itself -> cycle on second visit
        IThemeStyle? theme = null;
        theme = new ThemeStyle((f, v) => theme);

        // Act + Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _ = theme.GetStylesToApply(CreateFeature(), CreateViewport()).ToList());
        Assert.That(ex!.Message, Does.Contain("Cycle detected in style graph"));
    }
}
