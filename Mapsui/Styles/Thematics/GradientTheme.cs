// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;

namespace Mapsui.Styles.Thematics;

/// <summary>
/// The GradientTheme class defines a gradient color thematic rendering of features based by a numeric attribute.
/// </summary>
public class GradientTheme : Style, IThemeStyle
{
    /// <summary>
    /// Gets or sets the column name from where to get the attribute value
    /// </summary>
    public string ColumnName { get; init; }

    /// <summary>
    /// Gets or sets the minimum value of the gradient
    /// </summary>
    public double Min { get; init; }

    /// <summary>
    /// Gets or sets the maximum value of the gradient
    /// </summary>
    public double Max { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="IStyle">style</see> for the minimum value
    /// </summary>
    public IStyle MinStyle { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="IStyle">style</see> for the maximum value
    /// </summary>
    public IStyle MaxStyle { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="ColorBlend"/> used on labels
    /// </summary>
    public ColorBlend? TextColorBlend { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="ColorBlend"/> used on lines
    /// </summary>
    public ColorBlend? LineColorBlend { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="ColorBlend"/> used as Fill
    /// </summary>
    public ColorBlend? FillColorBlend { get; init; }

    /// <summary>
    /// Initializes a new instance of the GradientTheme class
    /// </summary>
    /// <param name="columnName">Name of column to extract the attribute</param>
    /// <param name="minValue">Minimum value</param>
    /// <param name="maxValue">Maximum value</param>
    /// <param name="minStyle">Color for minimum value</param>
    /// <param name="maxStyle">Color for maximum value</param>
    public GradientTheme(string columnName, double minValue, double maxValue, IStyle minStyle, IStyle maxStyle)
    {
        ColumnName = columnName;
        Min = minValue;
        Max = maxValue;
        MaxStyle = maxStyle;
        MinStyle = minStyle;
    }

    /// <summary>
    /// Returns the style based on a numeric DataColumn, where style
    /// properties are linearly interpolated between max and min values.
    /// </summary>
    /// <param name="row">Feature</param>
    /// <returns>A <see cref="IStyle">Style</see> calculated by a linear interpolation between the min/max styles</returns>
    public IStyle? GetStyle(IFeature row)
    {
        double attr;
        try { attr = Convert.ToDouble(row[ColumnName]); }
        catch { throw new Exception("Invalid Attribute type in Gradient Theme - Couldn't parse attribute (must be numerical)"); }
        if (MinStyle.GetType() != MaxStyle.GetType())
            throw new ArgumentException("MinStyle and MaxStyle must be of the same type");

        return (MinStyle, MaxStyle) switch
        {
            (LabelStyle minLabelStyle, LabelStyle maxLabelStyle)
                => ToInterpolatedLabelStyle(minLabelStyle, maxLabelStyle, attr, this),
            (SymbolStyle minSymbolStyle, SymbolStyle maxSymbolStyle)
                => ToInterpolatedSymbolStyle(minSymbolStyle, maxSymbolStyle, attr, this),
            (VectorStyle minVectorStyle, VectorStyle maxVectorStyle)
                => ToInterpolatedVectorStyle(minVectorStyle, maxVectorStyle, attr, this),
            _ => throw new NotSupportedException($"Style type '{MinStyle.GetType()}' and/or '{MinStyle.GetType()}' can not be used in the GradientTheme")
        };
    }

    private static VectorStyle ToInterpolatedVectorStyle(VectorStyle min, VectorStyle max, double value, GradientTheme instance)
    {
        var result = new VectorStyle();

        var fraction = Fraction(value, instance.Min, instance.Max);

        result.Enabled = fraction > 0.5 ? min.Enabled : max.Enabled;
        if (instance.FillColorBlend != null)
            result.Fill = new Brush { Color = instance.FillColorBlend.GetColor(fraction) };
        else if (min.Fill != null && max.Fill != null)
            result.Fill = InterpolateBrush(min.Fill, max.Fill, fraction);

        if (min.Line != null && max.Line != null)
            result.Line = InterpolatePen(min.Line, max.Line, fraction);
        if (instance.LineColorBlend != null && result.Line != null)
            result.Line.Color = instance.LineColorBlend.GetColor(fraction);

        if (min.Outline != null && max.Outline != null)
            result.Outline = InterpolatePen(min.Outline, max.Outline, fraction);

        return result;
    }

    private static SymbolStyle ToInterpolatedSymbolStyle(SymbolStyle min, SymbolStyle max, double value, GradientTheme instance)
    {
        var result = new SymbolStyle();

        var fraction = Fraction(value, instance.Min, instance.Max);

        result.ImageSource = (fraction > 0.5) ? min.ImageSource : max.ImageSource;
        result.SymbolOffset = fraction > 0.5 ? min.SymbolOffset ?? new Offset() : max.SymbolOffset ?? new Offset();
        // We don't interpolate the offset but let it follow the symbol instead
        result.SymbolScale = InterpolateDouble(min.SymbolScale, max.SymbolScale, fraction);

        return result;
    }

    private LabelStyle ToInterpolatedLabelStyle(LabelStyle min, LabelStyle max, double value, GradientTheme instance)
    {
        var result = new LabelStyle();

        var fraction = Fraction(value, instance.Min, instance.Max);

        result.CollisionDetection = min.CollisionDetection;
        result.Enabled = InterpolateBool(min.Enabled, max.Enabled, fraction);
        result.LabelColumn = InterpolateString(min.LabelColumn, max.LabelColumn, fraction);

        var fontSize = InterpolateDouble(min.Font.Size, max.Font.Size, fraction);
        result.Font = new Font { FontFamily = min.Font.FontFamily, Size = fontSize };

        if (min.BackColor != null && max.BackColor != null)
            result.BackColor = InterpolateBrush(min.BackColor, max.BackColor, fraction);

        result.ForeColor = TextColorBlend == null ?
            InterpolateColor(min.ForeColor, max.ForeColor, fraction) :
            TextColorBlend.GetColor(fraction);

        if (min.Halo != null && max.Halo != null)
            result.Halo = InterpolatePen(min.Halo, max.Halo, fraction);

        var x = InterpolateDouble(min.Offset.X, max.Offset.X, fraction);
        var y = InterpolateDouble(min.Offset.Y, max.Offset.Y, fraction);
        result.Offset = new Offset { X = x, Y = y };
        result.LabelColumn = min.LabelColumn;

        return result;
    }

    private static double Fraction(double attr, double min, double max)
    {
        if (attr < min) return 0;
        if (attr > max) return 1;

        return (attr - min) / (max - min);
    }

    private static bool InterpolateBool(bool min, bool max, double fraction) => fraction > 0.5 ? max : min;

    private static string? InterpolateString(string? min, string? max, double fraction) => fraction > 0.5 ? max : min;

    private static double InterpolateDouble(double min, double max, double fraction) => (max - min) * fraction + min;

    private static Brush InterpolateBrush(Brush min, Brush max, double fraction) => new()
    {
        Color = InterpolateColor(min.Color ?? Color.Transparent, max.Color ?? Color.Transparent, fraction)
    };

    private static Pen InterpolatePen(Pen min, Pen max, double fraction) => new()
    {
        Color = InterpolateColor(min.Color, max.Color, fraction),
        Width = InterpolateDouble(min.Width, max.Width, fraction)
    };

    private static Color InterpolateColor(Color minColor, Color maxColor, double fraction)
    {
        if (Math.Abs(fraction - 1) < Utilities.Constants.Epsilon)
            return maxColor;
        if (Math.Abs(fraction - 0) < Utilities.Constants.Epsilon)
            return minColor;

        var r = (maxColor.R - minColor.R) * fraction + minColor.R;
        var g = (maxColor.G - minColor.G) * fraction + minColor.G;
        var b = (maxColor.B - minColor.B) * fraction + minColor.B;
        var a = (maxColor.A - minColor.A) * fraction + minColor.A;

        if (r > 255) r = 255;
        if (g > 255) g = 255;
        if (b > 255) b = 255;
        if (a > 255) a = 255;

        return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }
}
