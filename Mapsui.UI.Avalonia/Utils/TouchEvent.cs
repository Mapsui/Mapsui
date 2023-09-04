namespace Mapsui.UI.Avalonia.Utils;

internal class TouchEvent
{
    public long Id { get; }
    public MPoint Location { get; }
    public long Tick { get; }

    public TouchEvent(long id, MPoint screenPosition, long tick)
    {
        Id = id;
        Location = screenPosition;
        Tick = tick;
    }
}
