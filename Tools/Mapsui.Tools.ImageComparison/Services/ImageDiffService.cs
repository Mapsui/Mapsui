using System;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace Mapsui.Tools.ImageComparison.Services;

static class ImageDiffService
{
    public static Bitmap? LoadThumbnail(string path, int maxWidth = 140)
    {
        using var original = LoadSkBitmap(path);
        if (original is null) return null;

        var scale = (float)maxWidth / original.Width;
        var newHeight = (int)(original.Height * scale);
        using var resized = original.Resize(new SKImageInfo(maxWidth, newHeight), SKSamplingOptions.Default);
        return resized is null ? null : ToAvaloniaBitmap(resized);
    }

    public static SKBitmap? LoadSkBitmap(string path)
    {
        if (!File.Exists(path)) return null;
        return SKBitmap.Decode(path);
    }

    // Returns a bitmap with diffColor pixels where images differ, transparent elsewhere.
    public static (SKBitmap Overlay, int DiffPixels) ComputeDiffOverlay(SKBitmap original, SKBitmap generated, SKColor diffColor)
    {
        var w = Math.Max(original.Width, generated.Width);
        var h = Math.Max(original.Height, generated.Height);

        var pixels1 = GetPixels(original, w, h);
        var pixels2 = GetPixels(generated, w, h);

        var overlayPixels = new SKColor[w * h];
        var diffCount = 0;
        for (var i = 0; i < overlayPixels.Length; i++)
        {
            if (pixels1[i] != pixels2[i])
            {
                overlayPixels[i] = diffColor;
                diffCount++;
            }
            // matching pixels stay transparent (default SKColor is (0,0,0,0))
        }

        var overlay = new SKBitmap(w, h);
        overlay.Pixels = overlayPixels;
        return (overlay, diffCount);
    }

    public static Bitmap ToAvaloniaBitmap(SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream(data.ToArray());
        return new Bitmap(ms);
    }

    static SKColor[] GetPixels(SKBitmap src, int w, int h)
    {
        if (src.Width == w && src.Height == h) return src.Pixels;
        // Pad to target size with transparent pixels
        using var padded = new SKBitmap(w, h);
        using var canvas = new SKCanvas(padded);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(src, 0, 0);
        return padded.Pixels;
    }
}
