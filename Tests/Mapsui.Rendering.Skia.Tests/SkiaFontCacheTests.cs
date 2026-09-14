using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class SkiaFontCacheTests
{
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
