using Mapsui.Utilities;

namespace Mapsui.Manipulations;
public record struct ScreenPosition(double X, double Y)
{
    public readonly ScreenPosition Offset(double offsetX, double offsetY) => new(X + offsetX, Y + offsetY);
    public readonly double Distance(ScreenPosition position) => Algorithms.Distance(X, Y, position.X, position.Y);
}
