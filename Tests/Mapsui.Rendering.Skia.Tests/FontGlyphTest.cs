using NUnit.Framework;
using SkiaSharp;
using System.IO;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class FontGlyphTest
{
    [Test]
    public async System.Threading.Tasks.Task CheckNotoSansSCGlyphsAsync()
    {
        var source = "embedded://Mapsui.Samples.Common.Resources.Fonts.NotoSansSC-Regular.ttf";
        var fontSource = new Mapsui.Styles.FontSource { Source = source };
        var cache = new Mapsui.Styles.FontSourceCache();
        await cache.TryRegisterAsync(fontSource.SourceId, source);
        var bytes = cache.Get(fontSource);
        Assert.That(bytes, Is.Not.Null, "Font bytes should be cached");
        await TestContext.Out.WriteLineAsync($"Bytes loaded: {bytes!.Length}");
        using var stream = new MemoryStream(bytes);
        var tf = SKTypeface.FromStream(stream);
        await TestContext.Out.WriteLineAsync($"Typeface family: {tf?.FamilyName ?? "NULL"}");
        Assert.That(tf, Is.Not.Null, "SKTypeface.FromStream should return non-null");
        int[] codepoints = [0x6B22, 0x8FCE, 0x4F7F, 0x7528];
        foreach (var cp in codepoints)
        {
            var g = tf!.GetGlyph(cp);
            await TestContext.Out.WriteLineAsync($"U+{cp:X4} ({(char)cp}): glyph id = {g}");
            Assert.That(g, Is.GreaterThan(0), $"Glyph for U+{cp:X4} must be > 0");
        }
    }

    [Test]
    public async System.Threading.Tasks.Task CheckNotoSansSCViaRenderServiceAsync()
    {
        // Simulate what the regression test does: create a Map with the sample,
        // call FetchAllFontDataAsync on its RenderService, then check the cache.
        var sample = new Mapsui.Samples.Common.Maps.Widgets.CustomFontWidgetSample();
        using var map = await sample.CreateMapAsync();
        await map.RenderService.FontSourceCache.FetchAllFontDataAsync();

        var source = "embedded://Mapsui.Samples.Common.Resources.Fonts.NotoSansSC-Regular.ttf";
        var fontSource = new Mapsui.Styles.FontSource { Source = source };
        var bytes = map.RenderService.FontSourceCache.Get(fontSource);
        await TestContext.Out.WriteLineAsync($"Bytes via RenderService: {bytes?.Length.ToString() ?? "NULL"}");
        Assert.That(bytes, Is.Not.Null, "RenderService cache should have NotoSansSC bytes after FetchAllFontDataAsync");
    }

    [Test]
    public async System.Threading.Tasks.Task RenderChineseWithNotoSansSCAsync()
    {
        // Load font bytes via the same cache pipeline as the renderer
        var source = "embedded://Mapsui.Samples.Common.Resources.Fonts.NotoSansSC-Regular.ttf";
        var fontSource = new Mapsui.Styles.FontSource { Source = source };
        var cache = new Mapsui.Styles.FontSourceCache();
        await cache.TryRegisterAsync(fontSource.SourceId, source);
        var bytes = cache.Get(fontSource);
        Assert.That(bytes, Is.Not.Null);

        using var stream = new MemoryStream(bytes!);
        using var tf = SKTypeface.FromStream(stream);
        Assert.That(tf, Is.Not.Null);
        await TestContext.Out.WriteLineAsync($"Typeface: {tf!.FamilyName}, glyphs: {tf.GlyphCount}");

        // Render "欢迎使用 Mapsui" to a small bitmap
        const int w = 300, h = 40;
        using var surface = SKSurface.Create(new SKImageInfo(w, h));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Purple);

        using var font = new SKFont { Typeface = tf, Size = 20 };
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText("欢迎使用 Mapsui", 5, 28, SKTextAlign.Left, font, paint);

        // Save for inspection
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NotoSansSCRender.png");
        System.IO.File.WriteAllBytes(path, data.ToArray());
        await TestContext.Out.WriteLineAsync($"Rendered to: {path}");

        // Assert: pixels in the left portion should include white (CJK chars rendered)
        using var pixels = image.PeekPixels();
        bool foundNonPurple = false;
        for (var x = 0; x < 130 && !foundNonPurple; x++)
            for (var y = 0; y < h; y++)
            {
                var c = pixels.GetPixelColor(x, y);
                if (c.Red > 200 && c.Green > 200 && c.Blue > 200) { foundNonPurple = true; break; }
            }

        Assert.That(foundNonPurple, Is.True,
            $"CJK glyphs should render as white pixels — typeface '{tf.FamilyName}' has the glyphs but DrawText produced no output");
    }
}
