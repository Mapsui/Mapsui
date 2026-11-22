using Mapsui.Experimental.VectorTiles.VexTileCopies;
using NUnit.Framework;
using SkiaSharp;
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

    [Test]
    public void TransformText_WordWrapWithMaxWidthZero_DoesNotThrow()
    {
        using var canvas = new SkiaCanvas(256, 256);
        // text-max-width 0: not even one character fits, BreakText used to throw.
        var brush = CreateBrush(textMaxWidth: 0.0);

        var result = canvas.TransformText("Hello World", brush, wordWrap: true);

        Assert.That(result, Is.EqualTo("Hello World"));
    }

    [Test]
    public void DrawPolygon_WithLineColor_StrokesTheOutline()
    {
        using var canvas = new SkiaCanvas(64, 64);
        var brush = CreateBrush(layerType: "fill");
        brush.Paint.FillColor = SKColors.Transparent;   // only the outline shows
        brush.Paint.LineColor = SKColors.Red;           // fill-outline-color
        brush.Paint.LineWidth = 2.0;

        canvas.DrawPolygon([new(10, 10), new(50, 10), new(50, 50), new(10, 50), new(10, 10)], brush);

        using var image = canvas.ToSKImage();
        using var bitmap = SKBitmap.FromImage(image);
        Assert.That(bitmap.GetPixel(30, 10).Red, Is.GreaterThan(200));   // on the edge
        Assert.That(bitmap.GetPixel(30, 30).Alpha, Is.EqualTo(0));       // inside stays empty
    }

    [Test]
    public void DrawPolygon_WithFillOpacity_AppliesItToTheOutline()
    {
        using var canvas = new SkiaCanvas(64, 64);
        var brush = CreateBrush(layerType: "fill");
        brush.Paint.FillColor = SKColors.Transparent;
        brush.Paint.FillOpacity = 0.5;
        brush.Paint.LineColor = SKColors.Red;
        brush.Paint.LineWidth = 4.0;

        canvas.DrawPolygon([new(10, 10), new(50, 10), new(50, 50), new(10, 50), new(10, 10)], brush);

        using var image = canvas.ToSKImage();
        using var bitmap = SKBitmap.FromImage(image);
        Assert.That(bitmap.GetPixel(30, 10).Alpha, Is.InRange(100, 155));   // about half
    }

    [Test]
    public void DrawPolygon_LineLayerOnPolygonFeature_DoesNotStrokeTheOutline()
    {
        // Line layers on polygon features (e.g. a dashed park outline) are drawn through
        // DrawPolygon too. Their LineColor comes from line-color, not fill-outline-color.
        using var canvas = new SkiaCanvas(64, 64);
        var brush = CreateBrush(layerType: "line");
        brush.Paint.FillColor = SKColors.Transparent;
        brush.Paint.LineColor = SKColors.Red;
        brush.Paint.LineWidth = 2.0;

        canvas.DrawPolygon([new(10, 10), new(50, 10), new(50, 50), new(10, 50), new(10, 10)], brush);

        using var image = canvas.ToSKImage();
        using var bitmap = SKBitmap.FromImage(image);
        Assert.That(bitmap.GetPixel(30, 10).Alpha, Is.EqualTo(0));
    }

    private static Brush CreateBrush(TextTransform transform = TextTransform.None, double textMaxWidth = 10.0,
        string? layerType = null) =>
        new Brush
        {
            Text = string.Empty,
            Layer = layerType is null ? null : new Layer { Type = layerType },
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
