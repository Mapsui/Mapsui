namespace Mapsui.UI;

public class EditMouseArgs
{
    public EditMouseArgs(MPoint screenPosition, bool leftButton, int clickCount)
    {
        ScreenPosition = screenPosition;
        LeftButton = leftButton;
        ClickCount = clickCount;
    }
    
    public int ClickCount { get; }
    public bool Handled { get; set; }
    public MPoint ScreenPosition { get; }
    public bool LeftButton { get; }
}
