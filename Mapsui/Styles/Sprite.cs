namespace Mapsui.Styles;

public class Sprite(int x, int y, int width, int height, float pixelRatio)
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public int Width { get; } = width;
    public int Height { get; } = height;
    public float PixelRatio { get; } = pixelRatio;

}
