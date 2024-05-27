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
public class IconButtonWidget : BoxWidget
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

    private string? _imageSource;

    /// <summary>
    /// The image to show as button
    /// </summary>
    public string? ImageSource
    {
        get => _imageSource;
        set
        {
            if (_imageSource == value)
                return;

            if (!string.IsNullOrEmpty(value))
                ImageSourceInitializer.Add(value);
            _imageSource = value;
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

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        return Tapped(this, e);
    }
}
