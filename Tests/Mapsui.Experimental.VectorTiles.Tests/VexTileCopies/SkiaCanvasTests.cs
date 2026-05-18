using Mapsui.Experimental.VectorTiles.VexTileCopies;
using NUnit.Framework;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VexTile.Renderer.Mvt.AliFlux.Enums;

namespace Mapsui.Experimental.VectorTiles.Tests.VexTileCopies;

[TestFixture]
public class SkiaCanvasTests
{
    [Test]
    public void TransformText_WithWordWrapFalse_NeverContainsNewline()
    {
        using var canvas = new SkiaCanvas(256, 256);
        // Very narrow limit — would cause word-breaking if wordWrap were true.
        var brush = CreateBrush(textMaxWidth: 1.0);

        var result = canvas.TransformText("Hello World This Is A Long Street Name", brush, wordWrap: false);

        Assert.That(result, Does.Not.Contain("\n"));
    }

    [Test]
    public void TransformText_WithWordWrapFalse_PreservesText()
    {
        using var canvas = new SkiaCanvas(256, 256);
        var brush = CreateBrush();

        var result = canvas.TransformText("Hello World", brush, wordWrap: false);

        Assert.That(result, Is.EqualTo("Hello World"));
    }

    [Test]
    public void TransformText_UppercaseTransform_WithWordWrapFalse_ConvertsToUpper()
    {
        using var canvas = new SkiaCanvas(256, 256);
        var brush = CreateBrush(transform: TextTransform.Uppercase);

        var result = canvas.TransformText("hello world", brush, wordWrap: false);

        Assert.That(result, Is.EqualTo("HELLO WORLD"));
    }

    [Test]
    public void TransformText_LowercaseTransform_WithWordWrapFalse_ConvertsToLower()
    {
        using var canvas = new SkiaCanvas(256, 256);
        var brush = CreateBrush(transform: TextTransform.Lowercase);

        var result = canvas.TransformText("HELLO WORLD", brush, wordWrap: false);

        Assert.That(result, Is.EqualTo("hello world"));
    }

    [Test]
    public void TransformText_EmptyText_ReturnsEmpty()
    {
        using var canvas = new SkiaCanvas(256, 256);
        var brush = CreateBrush();

        var result = canvas.TransformText(string.Empty, brush, wordWrap: false);

        Assert.That(result, Is.Empty);
    }

    private static Brush CreateBrush(TextTransform transform = TextTransform.None, double textMaxWidth = 10.0) =>
        new Brush
        {
            Text = string.Empty,
            Paint = new Paint
            {
                TextFont = ["Arial", "Arial Unicode MS Regular"],
                TextSize = 16.0,
                TextMaxWidth = textMaxWidth,
                TextTransform = transform,
                LineDashArray = [],
            }
        };
}
