using Mapsui.Widgets;
using System;

namespace Mapsui.Styles;

/// <summary>
/// Type of CalloutStyle
/// </summary>
public enum CalloutType
{
    /// <summary>
    /// Only one line is shown
    /// </summary>
    Single,
    /// <summary>
    /// Header and detail is shown
    /// </summary>
    Detail,
    /// <summary>
    /// Content is custom, the bitmap given in Content is shown
    /// </summary>
    Image,
}

/// <summary>
/// Determines, where the pointer is
/// </summary>
public enum TailAlignment
{
    /// <summary>
    /// Callout tail is at bottom side of bubble
    /// </summary>
    Bottom,
    /// <summary>
    /// Callout tail is at left side of bubble
    /// </summary>
    Left,
    /// <summary>
    /// Callout tail is at top side of bubble
    /// </summary>
    Top,
    /// <summary>
    /// Callout tail is at right side of bubble
    /// </summary>
    Right,
}

/// <summary>
/// A CalloutStyle shows a callout or InfoWindow in Google Maps
/// </summary>
/// <remarks>
/// There are three different types of Callouts
/// 1. Type = CalloutType.Single
///    The text in Title will be shown
/// 2. Type = CalloutType.Detail
///    The text in Title and SubTitle will be shown
/// 3. Type = CalloutType.Custom
///    The bitmap with ID in Content will be shown
/// </remarks>
public class CalloutStyle : SymbolStyle
{
    private CalloutType _type = CalloutType.Single;
    private double _rotation;
    private string? _title;
    private string? _subtitle;
    private Alignment _titleTextAlignment;
    private Alignment _subtitleTextAlignment;
    private double _spacing;
    private double _maxWidth;
    private Color? _titleFontColor;
    private Color? _subtitleFontColor;

    public string ImageIdOfCallout { get; private set; } = Guid.NewGuid().ToString();
    public string ImageIdOfCalloutContent { get; private set; } = Guid.NewGuid().ToString();

    public static new double DefaultWidth { get; set; } = 100;
    public static new double DefaultHeight { get; set; } = 30;

    /// <summary>
    /// Type of Callout
    /// </summary>
    /// <remarks>
    /// Could be Single, Detail or Image.
    /// </remarks>
    public CalloutType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the rotation of the Callout in degrees (clockwise is positive)
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                SymbolRotation = _rotation;
            }
        }
    }

    /// <summary>
    /// Content of Callout title label
    /// </summary>
    public string? Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Font color to render title
    /// </summary>
    public Color? TitleFontColor
    {
        get => _titleFontColor;
        set
        {
            _titleFontColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Text alignment of title
    /// </summary>
    public Alignment TitleTextAlignment
    {
        get => _titleTextAlignment;
        set
        {
            if (_titleTextAlignment != value)
            {
                _titleTextAlignment = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Content of Callout subtitle label
    /// </summary>
    public string? Subtitle
    {
        get => _subtitle;
        set
        {
            if (_subtitle != value)
            {
                _subtitle = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Font color to render subtitle
    /// </summary>
    public Color? SubtitleFontColor
    {
        get => _subtitleFontColor;
        set
        {
            _subtitleFontColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Text alignment of subtitle
    /// </summary>
    public Alignment SubtitleTextAlignment
    {
        get => _subtitleTextAlignment;
        set
        {
            if (_subtitleTextAlignment != value)
            {
                _subtitleTextAlignment = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Space between Title and Subtitle of Callout
    /// </summary>
    public double Spacing
    {
        get => _spacing;
        set
        {
            if (_spacing != value)
            {
                _spacing = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// MaxWidth for Title and Subtitle of Callout
    /// </summary>
    public double MaxWidth
    {
        get => _maxWidth;
        set
        {
            if (_maxWidth != value)
            {
                _maxWidth = value;
                Invalidate();
            }
        }
    }

    private Font _titleFont = new();
    private Font _subtitleFont = new();
    private CalloutBalloonDefinition _balloonDefinition = new();

    public Font TitleFont
    {
        get => _titleFont;
        set
        {
            _titleFont = value;
            Invalidate();
        }
    }

    public Font SubtitleFont
    {
        get => _subtitleFont;
        set
        {
            _subtitleFont = value;
            Invalidate();
        }
    }

    public CalloutBalloonDefinition BalloonDefinition
    {
        get => _balloonDefinition;
        set
        {
            _balloonDefinition = value;
            Invalidate();
        }
    }

    public void Invalidate()
    {
        ImageIdOfCalloutContent = Guid.NewGuid().ToString();
        ImageIdOfCallout = Guid.NewGuid().ToString();
    }
}
