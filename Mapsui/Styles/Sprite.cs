using System;
using System.Threading.Tasks;

namespace Mapsui.Styles;

public class Sprite
{
    public int Atlas { get; private set; } = -1;
    public Uri? AtlasPath { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public float PixelRatio { get; }
    public int BitmapId { get; set; } = -1;

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

    public Sprite(Uri atlasPath, MPoint p, Size s, float pixelRatio) : this(atlasPath, (int)p.X, (int)p.Y, (int)s.Width, (int)s.Height, pixelRatio)
    {
    }

    public async Task LoadBitmapIdAsync(IBitmapRegistry bitmapRegistry)
    {
        if (Atlas >= 0)
        {
            return;
        }

        if (AtlasPath != null)
        {
            Atlas = await bitmapRegistry.RegisterAsync(AtlasPath).ConfigureAwait(false);
        }
    }
}
