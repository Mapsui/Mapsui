using System;
using Mapsui.Logging;

namespace Mapsui.Styles;

public class Sprite
{
    public int Atlas { get; internal set; } = -1;
    public Uri? AtlasPath { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public float PixelRatio { get; }
    public object? Data { get; set; }

    public Sprite(int atlas, int x, int y, int width, int height, float pixelRatio)
    {
        Atlas = atlas;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        PixelRatio = pixelRatio;
    }

    public Sprite(Uri atlasPath, int x, int y, int width, int height, float pixelRatio)
    {
        AtlasPath = atlasPath;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        PixelRatio = pixelRatio;
    }

    public Sprite(int atlas, MPoint p, Size s, float pixelRatio) : this(atlas, (int)p.X, (int)p.Y, (int)s.Width, (int)s.Height, pixelRatio)
    {
    }

    public async void LoadBitmapIdAsync(IBitmapRegistry bitmapRegistry)
    {
        if (Atlas >= 0)
        {
            return;
        }

        if (AtlasPath != null)
        {
            try
            {
                Atlas = await bitmapRegistry.RegisterAsync(AtlasPath).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }
    }
}
