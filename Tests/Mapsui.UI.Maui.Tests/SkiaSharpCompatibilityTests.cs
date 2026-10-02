using Mapsui.Rendering.Skia;
using Mapsui.Rendering;
using Mapsui.Styles;
using NUnit.Framework;
using SkiaSharp;
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
    }
}
