using Mapsui.Manipulations;
using Microsoft.UI.Input;

namespace Mapsui.UI.WinUI.Extensions;
public static class PointerPointExtensions
{
    public static ScreenPosition ToScreenPosition(this PointerPoint pointerPoint)
    {
        return new ScreenPosition(pointerPoint.Position.X, pointerPoint.Position.Y);
    }
}
