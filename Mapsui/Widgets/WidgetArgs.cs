namespace Mapsui.Widgets;

public class WidgetArgs
{
    public WidgetArgs(int clickCount, bool leftButton, bool shift)
    {
        ClickCount = clickCount;
        LeftButton = leftButton;
        Shift = shift;
    }

    public int ClickCount { get; }
    public bool LeftButton { get; }
    public bool Shift { get; }
}
