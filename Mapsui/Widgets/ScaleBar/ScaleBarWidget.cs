using Mapsui.Logging;
using Mapsui.Projections;
using Mapsui.Styles;
using System;
using System.Collections.Generic;

namespace Mapsui.Widgets.ScaleBar;

/// <summary>
/// A ScaleBarWidget displays the ratio of a distance on the map to the corresponding distance on the ground.
/// It uses always the center of a given Viewport to calc this ratio.
///
/// Usage
/// To show a ScaleBarWidget, add a instance of the ScaleBarWidget to Map.Widgets by
/// 
///   map.Widgets.Add(new ScaleBarWidget(map));
///   
/// Customize
/// ScaleBarMode: Determines, how much scalebars are shown. Could be Single or Both.
/// SecondaryUnitConverter: First UnitConverter for upper scalebar. There are UnitConverters for metric, imperial and nautical units.
/// SecondaryUnitConverter = NauticalUnitConverter.Instance });
/// MaxWidth: Maximal width of the scalebar. Real width could be smaller.
/// HorizontalAlignment: Where the ScaleBarWidget is shown. Could be Left, Right, Center or Position.
/// VerticalAlignment: Where the ScaleBarWidget is shown. Could be Top, Bottom, Center or Position.
/// PositionX: If HorizontalAlignment is Position, this value determines the distance to the left
/// PositionY: If VerticalAlignment is Position, this value determines the distance to the top
/// TextColor: Color for text and lines
/// Halo: Color used around text and lines, so the scalebar is better visible
/// TextAlignment: Alignment of scalebar text to the lines. Could be Left, Right or Center
/// TextMargin: Space between text and lines of scalebar
/// Font: Font which is used to draw text
/// TickLength: Length of the ticks at scalebar
/// </summary>
public class ScaleBarWidget : BaseWidget
{
    private readonly Map? _map;
    private readonly IProjection? _projection;
    // Instead of using this property we could initialize _projection with ProjectionDefaults.Projection
    // in the constructor but in that way the overriding of ProjectionDefaults.Projection would not have 
    // effect if it was set after the ScaleBarWidget was constructed.
    private IProjection Projection => _projection ?? ProjectionDefaults.Projection;
    ///
    /// Default position of the scale bar.
    ///
    private static readonly HorizontalAlignment _defaultScaleBarHorizontalAlignment = HorizontalAlignment.Left;
    private static readonly VerticalAlignment _defaultScaleBarVerticalAlignment = VerticalAlignment.Bottom;
    private static readonly Alignment _defaultScaleBarAlignment = Alignment.Left;
    private static readonly ScaleBarMode _defaultScaleBarMode = ScaleBarMode.Single;
    private static readonly Font _defaultFont = new() { FontFamily = "Arial", Size = 10 };


    public ScaleBarWidget(Map map, IProjection? projection = null)
    {
        _map = map;
        _projection = projection;

        HorizontalAlignment = _defaultScaleBarHorizontalAlignment;
        VerticalAlignment = _defaultScaleBarVerticalAlignment;

        _maxWidth = 100;
        Height = 100;
        _textAlignment = _defaultScaleBarAlignment;
        _scaleBarMode = _defaultScaleBarMode;

        _unitConverter = MetricUnitConverter.Instance;
    }

    private double _maxWidth;

    /// <summary>
    /// Maximum usable width for scalebar. The real used width could be less, because we 
    /// want only integers as text.
    /// </summary>
    public double MaxWidth
    {
        get => _maxWidth;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_maxWidth == value)
                return;

            _maxWidth = value;
            Invalidate();
        }
    }

    private Color _textColor = new(0, 0, 0);

    /// <summary>
    /// Foreground color of scalebar and text
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

    private Color _haloColor = new(255, 255, 255);

    /// <summary>
    /// Halo color of scalebar and text, so that it is better visible
    /// </summary>
    public Color Halo
    {
        get => _haloColor;
        set
        {
            if (_haloColor == value)
                return;
            _haloColor = value;
            Invalidate();
        }
    }

    public double Scale { get; } = 1;

    /// <summary>
    /// Stroke width for lines
    /// </summary>
    public double StrokeWidth { get; set; } = 2;

    /// <summary>
    /// Stroke width for halo of lines
    /// </summary>
    public double StrokeWidthHalo { get; set; } = 4;

    /// <summary>
    /// Length of the ticks
    /// </summary>
    public double TickLength { get; set; } = 3;

    private Alignment _textAlignment;

    /// <summary>
    /// Alignment of text of scalebar
    /// </summary>
    public Alignment TextAlignment
    {
        get => _textAlignment;
        set
        {
            if (_textAlignment == value)
                return;

            _textAlignment = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Margin between end of tick and text
    /// </summary>
    public static double TextMargin => 1;

    private Font? _font = _defaultFont;

    /// <summary>
    /// Font to use for drawing text
    /// </summary>
    public Font? Font
    {
        get => _font ?? _defaultFont;
        set
        {
            if (_font == value)
                return;

            _font = value;
            Invalidate();
        }
    }

    private IUnitConverter _unitConverter;

    /// <summary>
    /// Normal unit converter for upper text. Default is MetricUnitConverter.
    /// </summary>
    public IUnitConverter UnitConverter
    {
        get => _unitConverter;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (_unitConverter == value)
                return;

            _unitConverter = value;
            Invalidate();
        }
    }

    private IUnitConverter? _secondaryUnitConverter;

    /// <summary>
    /// Secondary unit converter for lower text if ScaleBarMode is Both. Default is ImperialUnitConverter.
    /// </summary>
    public IUnitConverter? SecondaryUnitConverter
    {
        get => _secondaryUnitConverter;
        set
        {
            if (_secondaryUnitConverter == value)
            {
                return;
            }

            _secondaryUnitConverter = value;
            Invalidate();
        }
    }

    private ScaleBarMode _scaleBarMode;

    /// <summary>
    /// ScaleBarMode of scalebar. Could be Single to show only one or Both for showing two units.
    /// </summary>
    public ScaleBarMode ScaleBarMode
    {
        get => _scaleBarMode;
        set
        {
            if (_scaleBarMode == value)
            {
                return;
            }

            _scaleBarMode = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Draw a rectangle around the scale bar for testing
    /// </summary>
    public bool ShowEnvelop { get; set; }

    /// <summary>
    /// Calculates the length and text for both scalebars
    /// </summary>
    /// <returns>
    /// Length of upper scalebar
    /// Text of upper scalebar
    /// Length of lower scalebar
    /// Text of lower scalebar
    /// </returns>
    public (double scaleBarLength1, string? scaleBarText1, double scaleBarLength2, string? scaleBarText2)
        GetScaleBarLengthAndText(Viewport viewport)
    {
        if (_map?.CRS == null) return (0, null, 0, null);

        double length1;
        string text1;

        (length1, text1) = CalculateScaleBarLengthAndValue(_map.CRS, Projection, viewport, MaxWidth, UnitConverter);

        double length2;
        string? text2;

        if (SecondaryUnitConverter != null)
            (length2, text2) = CalculateScaleBarLengthAndValue(_map.CRS, Projection, viewport, MaxWidth, SecondaryUnitConverter);
        else
            (length2, text2) = (0, null);

        return (length1, text1, length2, text2);
    }

    /// <summary>
    /// Get pairs of points, which determine start and stop of the lines used to draw the scalebar
    /// </summary>
    /// <param name="viewport">The viewport of the map</param>
    /// <param name="scaleBarLength1">Length of upper scalebar</param>
    /// <param name="scaleBarLength2">Length of lower scalebar</param>
    /// <param name="stroke">Width of line</param>
    /// <returns>Array with pairs of Points. First is always the start point, the second is the end point.</returns>
    public IReadOnlyList<MPoint> GetScaleBarLinePositions(Viewport viewport, double scaleBarLength1, double scaleBarLength2, double stroke)
    {
        var points = new List<MPoint>();

        var drawNoSecondScaleBar = ScaleBarMode == ScaleBarMode.Single || ScaleBarMode == ScaleBarMode.Both && SecondaryUnitConverter == null;

        var maxScaleBarLength = Math.Max(scaleBarLength1, scaleBarLength2);

        UpdateEnvelope(_maxWidth, Height, viewport.Width, viewport.Height);

        var posX = (float)(Envelope?.MinX ?? 0.0);
        var posY = (float)(Envelope?.MinY ?? 0.0);

        var left = posX + stroke * 0.5f * Scale;
        var right = posX + _maxWidth - stroke * 0.5f * Scale;
        var center1 = posX + (_maxWidth - scaleBarLength1) / 2;
        var center2 = posX + (_maxWidth - scaleBarLength2) / 2;
        // Top position is Y in the middle of scale bar line
        var top = posY + (drawNoSecondScaleBar ? Height - stroke * 0.5f * Scale : Height * 0.5f);

        switch (TextAlignment)
        {
            case Alignment.Center:
                if (drawNoSecondScaleBar)
                {
                    points.Add(new MPoint(center1, top - TickLength * Scale));
                    points.Add(new MPoint(center1, top));
                    points.Add(new MPoint(center1, top));
                    points.Add(new MPoint(center1 + maxScaleBarLength, top));
                    points.Add(new MPoint(center1 + maxScaleBarLength, top));
                    points.Add(new MPoint(center1 + scaleBarLength1, top - TickLength * Scale));
                }
                else
                {
                    points.Add(new MPoint(Math.Min(center1, center2), top));
                    points.Add(new MPoint(Math.Min(center1, center2) + maxScaleBarLength, top));
                    points.Add(new MPoint(center1, top - TickLength * Scale));
                    points.Add(new MPoint(center1, top));
                    points.Add(new MPoint(center1 + scaleBarLength1, top - TickLength * Scale));
                    points.Add(new MPoint(center1 + scaleBarLength1, top));
                    points.Add(new MPoint(center2, top + TickLength * Scale));
                    points.Add(new MPoint(center2, top));
                    points.Add(new MPoint(center2 + scaleBarLength2, top + TickLength * Scale));
                    points.Add(new MPoint(center2 + scaleBarLength2, top));
                }
                break;
            case Alignment.Left:
                if (drawNoSecondScaleBar)
                {
                    points.Add(new MPoint(left, top));
                    points.Add(new MPoint(left + maxScaleBarLength, top));
                    points.Add(new MPoint(left, top - TickLength * Scale));
                    points.Add(new MPoint(left, top));
                    points.Add(new MPoint(left + scaleBarLength1, top - TickLength * Scale));
                    points.Add(new MPoint(left + scaleBarLength1, top));
                }
                else
                {
                    points.Add(new MPoint(left, top));
                    points.Add(new MPoint(left + maxScaleBarLength, top));
                    points.Add(new MPoint(left, top - TickLength * Scale));
                    points.Add(new MPoint(left, top + TickLength * Scale));
                    points.Add(new MPoint(left + scaleBarLength1, top - TickLength * Scale));
                    points.Add(new MPoint(left + scaleBarLength1, top));
                    points.Add(new MPoint(left + scaleBarLength2, top + TickLength * Scale));
                    points.Add(new MPoint(left + scaleBarLength2, top));
                }
                break;
            case Alignment.Right:
                if (drawNoSecondScaleBar)
                {

                    points.Add(new MPoint(right, top));
                    points.Add(new MPoint(right - maxScaleBarLength, top));
                    points.Add(new MPoint(right, top - TickLength * Scale));
                    points.Add(new MPoint(right, top));
                    points.Add(new MPoint(right - scaleBarLength1, top - TickLength * Scale));
                    points.Add(new MPoint(right - scaleBarLength1, top));
                }
                else
                {
                    points.Add(new MPoint(right, top));
                    points.Add(new MPoint(right - maxScaleBarLength, top));
                    points.Add(new MPoint(right, top - TickLength * Scale));
                    points.Add(new MPoint(right, top + TickLength * Scale));
                    points.Add(new MPoint(right - scaleBarLength1, top - TickLength * Scale));
                    points.Add(new MPoint(right - scaleBarLength1, top));
                    points.Add(new MPoint(right - scaleBarLength2, top + TickLength * Scale));
                    points.Add(new MPoint(right - scaleBarLength2, top));
                }
                break;
            default:
                throw new NotSupportedException($"TextAlignment {TextAlignment} is not supported");
        }

        return points;
    }

    /// <summary>
    /// Calculates the top-left-position of upper and lower text
    /// </summary>
    /// <param name="viewport">The viewport</param>
    /// <param name="textSize1">Size of upper text of scalebar</param>
    /// <param name="textSize2">Size of lower text of scalebar</param>
    /// <param name="stroke">Width of line</param>
    /// <returns>
    /// posX1 as left position of upper scalebar text
    /// posY1 as top position of upper scalebar text
    /// posX2 as left position of lower scalebar text
    /// posY2 as top position of lower scalebar text
    /// </returns>
    public (double posX1, double posY1, double posX2, double posY2) GetScaleBarTextPositions(
        Viewport viewport, MRect textSize1, MRect textSize2, double stroke)
    {
        var drawNoSecondScaleBar = ScaleBarMode == ScaleBarMode.Single || (ScaleBarMode == ScaleBarMode.Both && SecondaryUnitConverter == null);

        UpdateEnvelope(_maxWidth, Height, viewport.Width, viewport.Height);

        var posX = Envelope?.MinX ?? 0.0;
        var posY = Envelope?.MinY ?? 0.0;

        var left = posX + (stroke + TextMargin) * Scale;
        var right1 = posX + _maxWidth - (stroke + TextMargin) * Scale - textSize1.Width;
        var right2 = posX + _maxWidth - (stroke + TextMargin) * Scale - textSize2.Width;
        var top = posY;
        var bottom = posY + Height - textSize2.Height;

        switch (TextAlignment)
        {
            case Alignment.Center:
                if (drawNoSecondScaleBar)
                {
                    return (posX + (stroke + TextMargin) * Scale + (MaxWidth - 2.0f * (stroke + TextMargin) * Scale - textSize1.Width) / 2.0f,
                        top,
                        0,
                        0);
                }
                else
                {
                    return (posX + (stroke + TextMargin) * Scale + (MaxWidth - 2.0f * (stroke + TextMargin) * Scale - textSize1.Width) / 2.0f,
                            top,
                            posX + (stroke + TextMargin) * Scale + (MaxWidth - 2.0f * (stroke + TextMargin) * Scale - textSize2.Width) / 2.0f,
                            bottom);
                }
            case Alignment.Left:
                if (drawNoSecondScaleBar)
                {
                    return (left, top, 0, 0);
                }
                else
                {
                    return (left, top, left, bottom);
                }
            case Alignment.Right:
                if (drawNoSecondScaleBar)
                {
                    return (right1, top, 0, 0);
                }
                else
                {
                    return (right1, top, right2, bottom);
                }
            default:
                return (0, 0, 0, 0);
        }
    }

    public bool CanProject()
    {
        if (_map?.CRS == null)
        {
            Logger.Log(LogLevel.Warning, $"ScaleBarWidget can not draw because the {nameof(Map)}.{nameof(Map.CRS)} is not set");
            return false;
        }

        if (Projection == null)
        {
            Logger.Log(LogLevel.Warning, $"ScaleBarWidget can not draw because the {nameof(Map)}.{nameof(Projection)} is not set");
            return false;
        }

        if (Projection.IsProjectionSupported(_map.CRS, "EPSG:4326") != true)
        {
            Logger.Log(LogLevel.Warning, $"ScaleBarWidget can not draw because the projection between {_map.CRS} and EPSG:4326 is not supported");
            return false;
        }
        return true;
    }

    /// Calculates the required length and value of a scalebar
    ///
    /// @param viewport the Viewport to calculate for
    /// @param width of the scale bar in pixel to calculate for
    /// @param unitConverter the DistanceUnitConverter to calculate for
    /// @return scaleBarLength and scaleBarText
    private static (double scaleBarLength, string scaleBarText) CalculateScaleBarLengthAndValue(
        string CRS, IProjection projection, Viewport viewport, double width, IUnitConverter unitConverter)
    {
        // We have to calc the angle difference to the equator (angle = 0), 
        // because EPSG:3857 is only there 1 m. At other angles, we
        // should calculate the correct length.

        var (_, y) = projection.Project(CRS, "EPSG:4326", viewport.CenterX, viewport.CenterY); // clone or else you will project the original viewport center

        // Calc ground resolution in meters per pixel of viewport for this latitude
        var groundResolution = viewport.Resolution * Math.Cos(y / 180.0 * Math.PI);

        // Convert in units of UnitConverter
        groundResolution /= unitConverter.MeterRatio;

        var scaleBarValues = unitConverter.ScaleBarValues;

        double scaleBarLength = 0;
        var scaleBarValue = 0;

        foreach (var value in scaleBarValues)
        {
            scaleBarValue = value;
            scaleBarLength = scaleBarValue / groundResolution;
            if (scaleBarLength < width - 10)
            {
                break;
            }
        }

        var scaleBarText = unitConverter.GetScaleText(scaleBarValue);

        return (scaleBarLength, scaleBarText);
    }
}
