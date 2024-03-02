using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;
using System;

namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget that shows a button with an icon
/// </summary>
/// <remarks>
/// With this, the user could add buttons with SVG icons to the map.
/// 
/// Usage
/// To show a IconButtonWidget, add a instance of the IconButtonWidget to Map.Widgets by
/// 
///   map.Widgets.Add(new IconButtonWidget(map, picture));
///   
/// Customize
/// Picture: SVG image to display for button
/// Rotation: Value for rotation in degrees
/// Opacity: Opacity of button
/// </remarks>
public class IconButtonWidget : BoxWidget, ITouchableWidget
{
    public IconButtonWidget() : base()
    {
        BackColor = Color.Transparent;
    }

    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public Func<IconButtonWidget, WidgetEventArgs, bool> Tapped = (s, e) => false;

    private MRect _padding = new(0);

    /// <summary>
    /// Padding left and right for icon inside the Widget
    /// </summary>
    public MRect Padding
    {
        get => _padding;
        set
        {
            if (_padding == value)
              return;
            
            _padding = value;
            Invalidate();
        }
    }

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
            Invalidate();
        }
    }

    private object? _picture;

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
            Invalidate();
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
            Invalidate();
        }
    }

    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;

    public bool OnTapped(Navigator navigator, MPoint position, WidgetEventArgs e)
    {
        return Tapped(this, e);
    }

    public bool OnPointerPressed(Navigator navigator, MPoint position, WidgetEventArgs e)
    {
        return false;
    }

    public bool OnPointerMoved(Navigator navigator, MPoint position, WidgetEventArgs e)
    {
        return false;
    }
}
