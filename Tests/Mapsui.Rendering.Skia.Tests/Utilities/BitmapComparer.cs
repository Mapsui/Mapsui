﻿using SkiaSharp;
using System;
using System.IO;

namespace Mapsui.Rendering.Skia.Tests.Utilities;

public class BitmapComparer
{
    public static bool Compare(Stream? bitmapStream1, Stream? bitmapStream2, int allowedColorDistance = 0, double proportionCorrect = 1)
    {
        // The bitmaps in WPF can slightly differ from test to test. No idea why. So introduced proportion correct.

        long trueCount = 0;
        long falseCount = 0;

        if (bitmapStream1 == null && bitmapStream2 == null)
        {
            return true;
        }

        if (bitmapStream1 == null || bitmapStream2 == null)
        {
            return false;
        }

        bitmapStream1.Position = 0;
        bitmapStream2.Position = 0;

        using var skData1 = SKData.Create(bitmapStream1);
        var bitmap1 = SKBitmap.FromImage(SKImage.FromEncodedData(skData1));
        using var skData2 = SKData.Create(bitmapStream2);
        var bitmap2 = SKBitmap.FromImage(SKImage.FromEncodedData(skData2));

        if (bitmap1.Width != bitmap2.Width || bitmap1.Height != bitmap2.Height)
        {
            return false;
        }

        for (var x = 0; x < bitmap1.Width; x++)
        {
            for (var y = 0; y < bitmap1.Height; y++)
            {
                var color1 = bitmap1.GetPixel(x, y);
                var color2 = bitmap2.GetPixel(x, y);
                if (color1 == color2)
                    trueCount++;
                else
                {
                    if (CompareColors(color1, color2, allowedColorDistance))
                        trueCount++;
                    else
                        falseCount++;
                }
            }
        }

        var proportion = (double)(trueCount) / (trueCount + falseCount);
        return proportionCorrect <= proportion;
    }

    private static bool CompareColors(SKColor color1, SKColor color2, int allowedColorDistance)
    {
        if (color1.Alpha == 0 && color2.Alpha == 0) return true; // If both are transparent all colors are ignored
        if (Math.Abs(color1.Alpha - color2.Alpha) > allowedColorDistance) return false;
        if (Math.Abs(color1.Red - color2.Red) > allowedColorDistance) return false;
        if (Math.Abs(color1.Green - color2.Green) > allowedColorDistance) return false;
        if (Math.Abs(color1.Blue - color2.Blue) > allowedColorDistance) return false;
        return true;
    }
}
