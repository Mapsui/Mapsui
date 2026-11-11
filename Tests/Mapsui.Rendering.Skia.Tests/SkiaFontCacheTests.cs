using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class SkiaFontCacheTests
{
    [Test]
    public void FontWithoutFontFamilyGetsUsableTypeface()
    {
        // A null typeface used to resolve to the platform default on SkiaSharp 3, but SkiaSharp 4 stores
        // SKTypeface.Empty instead, which measures every string as zero width and draws nothing. See issue 3409.
        using var renderService = new RenderService();
        var font = new Font { Size = 12 }; // No FontFamily, which is the default for Font.

        using var holder = SkiaFontCache.GetOrCreate(font, renderService);

        Assert.That(holder.Instance.Typeface, Is.Not.Null);
        Assert.That(holder.Instance.Typeface.GlyphCount, Is.GreaterThan(0));
        Assert.That(holder.Instance.MeasureText("Mapsui"), Is.GreaterThan(0));
    }

    [Test]
    public void NullFontGetsUsableTypeface()
    {
        // Widget fonts such as LoggingWidget.Font and TextBoxWidget.Font are nullable and often left unset.
        using var renderService = new RenderService();

        using var holder = SkiaFontCache.GetOrCreate(null, 12, renderService);

        Assert.That(holder.Instance.Typeface, Is.Not.Null);
        Assert.That(holder.Instance.Typeface.GlyphCount, Is.GreaterThan(0));
        Assert.That(holder.Instance.MeasureText("Mapsui"), Is.GreaterThan(0));
    }

    [Test]
    public void ReusesFontForEquivalentValues()
    {
        using var renderService = new RenderService();
        var firstFont = new Font { FontFamily = "Arial", Size = 12, Bold = true };
        var secondFont = new Font { FontFamily = "Arial", Size = 12, Bold = true };

        object first;
        using (var holder = SkiaFontCache.GetOrCreate(firstFont, renderService))
            first = holder.Instance;

        using var secondHolder = SkiaFontCache.GetOrCreate(secondFont, renderService);

        Assert.That(secondHolder.Instance, Is.SameAs(first));
    }

    [Test]
    public void FontMutationSelectsDifferentCacheEntry()
    {
        using var renderService = new RenderService();
        var font = new Font { FontFamily = "Arial", Size = 12 };

        object first;
        using (var holder = SkiaFontCache.GetOrCreate(font, renderService))
            first = holder.Instance;

        font.Size = 14;
        using var secondHolder = SkiaFontCache.GetOrCreate(font, renderService);

        Assert.That(secondHolder.Instance, Is.Not.SameAs(first));
    }
}
