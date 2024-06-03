using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;
using System;

namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget that shows a button with an icon
/// </summary>
public class ImageButtonWidget : BoxWidget, IHasImageSource
{
    public ImageButtonWidget() : base()
    {
        BackColor = Color.Transparent;
    }

    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public Func<ImageButtonWidget, WidgetEventArgs, bool> Tapped = (s, e) => false;

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
