namespace Mapsui.Widgets;

public class WidgetArgs
{
    WidgetArgs(int clickCount, bool leftButton)
    {
        ClickCount = clickCount;
        LeftButton = leftButton;
    }

    public int ClickCount { get; }
    public bool LeftButton { get; }
}
