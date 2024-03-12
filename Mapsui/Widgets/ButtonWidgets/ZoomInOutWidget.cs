﻿using Mapsui.Styles;

namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget which shows two buttons (horizontal or vertical) with a "+" and a "-" sign.
/// With this, the user could zoom the map in and out.
/// 
/// Usage
/// To show a ZoomInOutWidget, add a instance of the ZoomInOutWidget to Map.Widgets by
/// 
///   map.Widgets.Add(new ZoomInOutWidget(map));
///   
/// Customize
/// Size: Height and Width of the buttons
/// Orientation: Orientation of the buttons. Could be Horizontal or Vertical. Vertical is default.
/// StrokeColor: Color of button frames
/// TextColor: Color of "+" and "-" signs
/// BackColor: Color of button background
/// Opacity: Opacity of buttons
/// ZoomFactor: Factor for changing Resolution. Default is 2;
/// </summary>
public class ZoomInOutWidget : BaseWidget
{
    private double _size = 40;

    /// <summary>
    /// Width and height of buttons
    /// </summary>
    public double Size
    {
        get => _size;
        set
        {
            if (_size == value)
                return;
            _size = value;
            Invalidate();
        }
    }

    private Orientation _orientation = Orientation.Vertical;

    /// <summary>
    /// Orientation of buttons
    /// </summary>
    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value)
                return;
            _orientation = value;
            Invalidate();
        }
    }

    private Color _strokeColor = new(192, 192, 192);

    /// <summary>
    /// Color of button frames
    /// </summary>
    public Color StrokeColor
    {
        get => _strokeColor;
        set
        {
            if (_strokeColor == value)
                return;
            _strokeColor = value;
            Invalidate();
        }
    }

    private Color _textColor = new(192, 192, 192);

    /// <summary>
    /// Color of "+" and "-" sign
    /// </summary>
    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (_textColor == value)
                return;
            _textColor = value;
            Invalidate();
        }
    }

    private Color _backColor = new(224, 224, 224);

    /// <summary>
    /// Color of background
    /// </summary>
    public Color BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor == value)
                return;
            _backColor = value;
            Invalidate();
        }
    }

    private double _opacity = 0.8f;

    /// <summary>
    /// Opacity of background, frame and signs
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set
        {
            if (_opacity == value)
                return;
            _opacity = value;
            Invalidate();
        }
    }

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        var result = base.OnTapped(navigator, e);

        if (result)
            return true;

        if (Envelope == null)
            return false;

        if (Orientation == Orientation.Vertical && e.Position.Y < Envelope.MinY + Envelope.Height * 0.5 ||
            Orientation == Orientation.Horizontal && e.Position.X < Envelope.MinX + Envelope.Width * 0.5)
        {
            navigator.ZoomIn(500);
        }
        else
        {
            navigator.ZoomOut(500);
        }

        return true;
    }
}
