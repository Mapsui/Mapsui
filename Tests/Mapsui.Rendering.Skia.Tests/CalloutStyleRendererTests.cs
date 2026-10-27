using Mapsui.Styles;
using NUnit.Framework;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class CalloutStyleRendererTests
{
    [Test]
    public void CalloutWithDefaultFontColorsIsVisible()
    {
        // A null font color renders fully transparent text, so CalloutStyle defaults these colors to black.
        using var renderService = new RenderService();
        using var picture = CalloutStyleRenderer.CreateCalloutContent(new CalloutStyle
        {
            Type = CalloutType.Detail,
            Title = "Title",
            Subtitle = "Subtitle",
            MaxWidth = 200,
        }, renderService);

        Assert.That(CountOpaquePixels(picture), Is.GreaterThan(0), "The callout text produced no pixels.");
    }

    [Test]
    public void CalloutWithClearedFontColorsIsInvisible()
    {
        // Assigning null stays expressible, and the renderer logs a warning for it.
        using var renderService = new RenderService();
        using var picture = CalloutStyleRenderer.CreateCalloutContent(new CalloutStyle
        {
            Type = CalloutType.Detail,
            Title = "Title",
            Subtitle = "Subtitle",
            MaxWidth = 200,
            TitleFontColor = null,
            SubtitleFontColor = null,
        }, renderService);

        Assert.That(CountOpaquePixels(picture), Is.EqualTo(0));
    }

    private static int CountOpaquePixels(SKPicture picture)
    {
        var bounds = picture.CullRect;
        var width = (int)System.Math.Ceiling(bounds.Width);
        var height = (int)System.Math.Ceiling(bounds.Height);
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
}
