using Mapsui.Styles;

namespace Mapsui.Widgets.ButtonWidgets;

public class OnOffButtonWidget : TextButtonWidget
{
    private bool _status = false;

    /// <summary>
    /// Status of button, where false is off and true is on
    /// </summary>
    public bool Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            UpdateBackColor();
            Invalidate();
        }
    }

    private Color _onColor = Color.LightGreen;

    /// <summary>
    /// Background color when button is on
    /// </summary>
    public Color OnColor
    { 
        get => _onColor; 
        set 
        { 
            if (_onColor == value) return;
            _onColor = value;
            UpdateBackColor();
            Invalidate();
        } 
    }

    private Color _offColor = Color.Red;

    /// <summary>
    /// Background color when button is off
    /// </summary>
    public Color OffColor
    {
        get => _offColor;
        set
        {
            if (_offColor == value) return;
            _offColor = value;
            UpdateBackColor();
            Invalidate();
        }
    }

    /// <summary>
    /// Handle touch to Widget
    /// </summary>
    /// <param name="navigator">Navigator used by map</param>
    /// <param name="position">Position of touch</param>
    /// <param name="args">Arguments for widget event</param>
    /// <returns>True, if touch is handled</returns>
    public override bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        Status = !Status;

        return base.HandleWidgetTouched(navigator, position, args);
    }

    private void UpdateBackColor()
    {
        BackColor = _status ? OnColor : OffColor;
    }
}
