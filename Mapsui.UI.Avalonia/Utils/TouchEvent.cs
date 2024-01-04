namespace Mapsui.UI.Avalonia.Utils;

internal class TouchEvent
{
    public MPoint Location { get; }

    public TouchEvent(MPoint screenPosition)
    {
        Location = screenPosition;
    }
}
