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

    protected bool Equals(Sprite other)
    {
        return Atlas == other.Atlas && X == other.X && Y == other.Y && Width == other.Width && Height == other.Height && PixelRatio.Equals(other.PixelRatio) && BitmapId == other.BitmapId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((Sprite)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Atlas;
            hashCode = (hashCode * 397) ^ X;
            hashCode = (hashCode * 397) ^ Y;
            hashCode = (hashCode * 397) ^ Width;
            hashCode = (hashCode * 397) ^ Height;
            hashCode = (hashCode * 397) ^ PixelRatio.GetHashCode();
            hashCode = (hashCode * 397) ^ BitmapId;
            return hashCode;
        }
    }

    public static bool operator ==(Sprite? left, Sprite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Sprite? left, Sprite? right)
    {
        return !Equals(left, right);
    }
}
