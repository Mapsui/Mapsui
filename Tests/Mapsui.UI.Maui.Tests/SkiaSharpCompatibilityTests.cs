using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering;
using Mapsui.Styles;
using NUnit.Framework;
using SkiaSharp;
using System;
using System.Reflection;

namespace Mapsui.UI.Maui.Tests;

[TestFixture]
public class SkiaSharpCompatibilityTests
{
    [Test]
    public void RendererRunsWithSkiaSharp4()
    {
        var managedVersion = typeof(SKCanvas).Assembly.GetName().Version;

        Assert.That(managedVersion, Is.Not.Null);
        Assert.That(managedVersion!.Major, Is.EqualTo(4));
        Assert.That(SkiaSharpVersion.Native.Major, Is.GreaterThanOrEqualTo(150));

        using var map = new Map();
        map.Navigator.SetSize(64, 64);
        var renderer = new MapRenderer();

        using var stream = renderer.RenderToBitmapStream(map);
        stream.Position = 0;
        using var image = SKImage.FromEncodedData(stream);

        Assert.That(image, Is.Not.Null);
        Assert.That(image!.Width, Is.EqualTo(64));
        Assert.That(image.Height, Is.EqualTo(64));
    }

    [Test]
    public void RichTextKitCalloutRunsWithSkiaSharp4AndMatchingHarfBuzz()
    {
        var harfBuzzVersion = typeof(HarfBuzzSharp.Buffer).Assembly
            .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        Assert.That(harfBuzzVersion, Is.Not.Null);
        Assert.That(harfBuzzVersion, Does.StartWith("14.2.1.102"));

        using var renderService = new RenderService();
        using var picture = CalloutStyleRenderer.CreateCalloutContent(new CalloutStyle
        {
            Type = CalloutType.Detail,
            Title = "Hello 🌍 مرحبا",
            Subtitle = "Wrapped bidirectional text 🎉",
            MaxWidth = 120,
        }, renderService);

        Assert.That(picture.CullRect.Width, Is.GreaterThan(0));
        Assert.That(picture.CullRect.Height, Is.GreaterThan(0));
        // A blank callout still reports a cull rect, so count the glyph pixels that were actually recorded.
        Assert.That(CountOpaquePixels(picture), Is.GreaterThan(0), "The callout text produced no pixels.");
    }

    [Test]
    public void LabelWithoutFontFamilyIsDrawnWithSkiaSharp4()
    {
        // SkiaSharp 4 stores SKTypeface.Empty for an unset typeface, which measures as zero width and draws
        // nothing, so a Font without a FontFamily must still resolve to a real typeface. See issue 3409.
        using var map = new Map();
        map.Layers.Add(new MemoryLayer("Labels")
        {
            Features = [new PointFeature(50, 50)],
            Style = new LabelStyle
            {
                Text = "Mapsui",
                Font = new Font { Size = 24 }, // No FontFamily, which is the default for Font.
                BackColor = null,
                Halo = null,
                ForeColor = Color.Black,
            },
        });
        map.Navigator.SetSize(200, 200);
        map.Navigator.CenterOnAndZoomTo(new MPoint(50, 50), 1);
        var mapRenderer = new MapRenderer();

        using var stream = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers, map.RenderService, Color.White);
        using var bitmap = SKBitmap.Decode(stream.ToArray());

        Assert.That(CountNonBackgroundPixels(bitmap), Is.GreaterThan(0), "The label produced no pixels.");
    }

    private static int CountOpaquePixels(SKPicture picture)
    {
        var bounds = picture.CullRect;
        var width = (int)Math.Ceiling(bounds.Width);
        var height = (int)Math.Ceiling(bounds.Height);
        if (width <= 0 || height <= 0)
            return 0;

        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.Translate(-bounds.Left, -bounds.Top);
            canvas.DrawPicture(picture);
        }

        var count = 0;
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                if (bitmap.GetPixel(x, y).Alpha > 0)
                    count++;
        return count;
    }

    private static int CountNonBackgroundPixels(SKBitmap bitmap)
    {
        var count = 0;
        for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.Alpha > 0 && (pixel.Red < 250 || pixel.Green < 250 || pixel.Blue < 250))
                    count++;
            }
        return count;
    }
}
