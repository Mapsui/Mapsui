using System;

namespace Mapsui.Widgets.ButtonWidget;

/// <summary>
/// Widget which shows a buttons
/// </summary>
/// <remarks>
/// With this, the user could add buttons with SVG icons to the map.
/// 
/// Usage
/// To show a ButtonWidget, add a instance of the ButtonWidget to Map.Widgets by
/// 
///   map.Widgets.Add(new ButtonWidget(map, picture));
///   
/// Customize
/// Picture: SVG image to display for button
/// Rotation: Value for rotation in degrees
/// Opacity: Opacity of button
/// </remarks>
public class ButtonWidget : TextBoxWidget, ITouchableWidget
{
    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? Touched;

    private string? _svgImage;

    /// <summary>
    /// SVG image to show for button
    /// </summary>
    public string? SvgImage
    {
        get => _svgImage;
        set
        {
            if (_svgImage == value)
                return;

            _svgImage = value;
            Picture = null;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Object for prerendered image. For internal use only.
    /// </summary>
    public object? Picture
    {
        get => _picture;
        set
        {
            if (Equals(value, _picture)) return;
            _picture = value;
            OnPropertyChanged();
        }
    }

    private double _rotation;

    /// <summary>
    /// Rotation of the SVG image
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation == value)
                return;
            _rotation = value;
            OnPropertyChanged();
        }
    }

    private float _opacity = 0.8f;
    private object? _picture;

    /// <summary>
    /// Opacity of background, frame and signs
    /// </summary>
    public float Opacity
    {
        get => _opacity;
        set
        {
            if (_opacity == value)
                return;
            _opacity = value;
            OnPropertyChanged();
        }
    }

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        Touched?.Invoke(this, args);

        return args.Handled;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }

    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;
}
