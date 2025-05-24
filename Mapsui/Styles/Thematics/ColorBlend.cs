// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Linq;

namespace Mapsui.Styles.Thematics;

/// <summary>
/// Defines arrays of colors and positions used for interpolating color blending in a multicolor gradient.
/// </summary>
public class ColorBlend
{
    private static int _rainbow7ColorCount = 7;

    /// <summary>
    /// Gets or sets an array of colors that represents the colors to use at corresponding positions along a gradient.
    /// </summary>
    public Color[]? Colors { get; set; }

    /// <summary>
    /// Gets or sets the positions along a gradient line.
    /// </summary>
    /// <value>An array of values that specify percentages of distance along the gradient line.</value>
    /// <remarks>
    /// <para>The elements of this array specify percentages of distance along the gradient line.
    /// For example, an element value of 0.2f specifies that this point is 20 percent of the total
    /// distance from the starting point. The elements in this array are represented by float
    /// values between 0.0f and 1.0f, and the first element of the array must be 0.0f and the
    /// last element must be 1.0f.</para>
    /// <pre>Along with the Colors property, this property defines a multicolor gradient.</pre>
    /// </remarks>
    public double[]? Positions { get; set; }

    internal ColorBlend() { }

    /// <summary>
    /// Initializes a new instance of the ColorBlend class.
    /// </summary>
    /// <param name="colors">An array of Color structures that represents the colors to use at corresponding positions along a gradient.</param>
    /// <param name="positions">An array of values that specify percentages of distance along the gradient line.</param>
    public ColorBlend(Color[] colors, double[] positions)
    {
        Colors = colors;
        Positions = positions;
    }

    /// <summary>
    /// Gets the color from the scale at position 'pos'.
    /// </summary>
    /// <remarks>If the position is outside the scale [0..1] only the fractional part
    /// is used (in other words the scale restarts for each integer-part).</remarks>
    /// <param name="pos">Position on scale between 0.0f and 1.0f</param>
    /// <returns>Color on scale</returns>
    public Color GetColor(double pos)
    {
        if (Colors == null || Positions == null)
            throw new ArgumentException("Colors and Positions needs to be set");
        if (Colors.Length != Positions.Length)
            throw new ArgumentException("Colors and Positions arrays must be of equal length");
        if (Colors.Length < 2)
            throw new ArgumentException("At least two colors must be defined in the ColorBlend");
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Positions[0] != 0f)
            throw new ArgumentException("First position value must be 0.0f");
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Positions[^1] != 1f)
            throw new ArgumentException("Last position value must be 1.0f");
        if (pos > 1 || pos < 0) pos -= Math.Floor(pos);
        var i = 1;
        while (i < Positions.Length && Positions[i] < pos)
            i++;
        var frac = (pos - Positions[i - 1]) / (Positions[i] - Positions[i - 1]);
        var red = (int)Math.Round((Colors[i - 1].R * (1 - frac) + Colors[i].R * frac));
        var green = (int)Math.Round((Colors[i - 1].G * (1 - frac) + Colors[i].G * frac));
        var blue = (int)Math.Round((Colors[i - 1].B * (1 - frac) + Colors[i].B * frac));
        var alpha = (int)Math.Round((Colors[i - 1].A * (1 - frac) + Colors[i].A * frac));

        return new Color { A = alpha, R = red, G = green, B = blue }; // Not sure how to assign in case of equal naming
    }

    /// <summary>
    /// Gets a linear gradient scale with seven colors making a rainbow from red to violet.
    /// </summary>
    /// <remarks>
    /// Colors span the following with an interval of 1/6:
    /// { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet }
    /// </remarks>
    public static ColorBlend Rainbow7 => new()
    {
        Positions = Enumerable.Range(0, _rainbow7ColorCount).Select(i => (double)i / (_rainbow7ColorCount - 1)).ToArray(),
        Colors = [
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.Green,
            Color.Blue,
            Color.Indigo,
            Color.Violet
        ]
    };

    /// <summary>
    /// Gets a linear gradient scale with five colors making a rainbow from red to blue.
    /// </summary>
    /// <remarks>
    /// Colors span the following with an interval of 0.25:
    /// { Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue }
    /// </remarks>
    public static ColorBlend Rainbow5 =>
        new([Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue], [0, 0.25, 0.5, 0.75, 1]);

    /// <summary>
    /// Gets a linear gradient scale from black to white
    /// </summary>
    public static ColorBlend BlackToWhite => new([Color.Black, Color.White], [0.0, 1.0]);

    /// <summary>
    /// Gets a linear gradient scale from white to black
    /// </summary>
    public static ColorBlend WhiteToBlack => new([Color.White, Color.Black], [0.0, 1.0]);

    /// <summary>
    /// Gets a linear gradient scale from red to green
    /// </summary>
    public static ColorBlend RedToGreen => new([Color.Red, Color.Green], [0.0, 1.0]);

    /// <summary>
    /// Gets a linear gradient scale from green to red
    /// </summary>
    public static ColorBlend GreenToRed => new([Color.Green, Color.Red], [0.0, 1.0]);

    /// <summary>
    /// Gets a linear gradient scale from blue to green
    /// </summary>
    public static ColorBlend BlueToGreen => new([Color.Blue, Color.Green], [0.0, 1.0]);

    /// <summary>
    /// Gets a linear gradient scale from green to blue
    /// </summary>
    public static ColorBlend GreenToBlue => new([Color.Green, Color.Blue], [0.0, 1.0]);

    /// <summary>
    /// Gets a linear gradient scale from red to blue
    /// </summary>
    public static ColorBlend RedToBlue => new([Color.Red, Color.Blue], [0.0, 1.0]);

    /// <summary>
    /// Gets a linear gradient scale from blue to red
    /// </summary>
    public static ColorBlend BlueToRed => new([Color.Blue, Color.Red], [0.0, 1.0]);

    /// <summary>
    /// Creates a linear gradient scale from two colors
    /// </summary>
    /// <param name="fromColor"></param>
    /// <param name="toColor"></param>
    /// <returns></returns>
    public static ColorBlend TwoColors(Color fromColor, Color toColor) => new([fromColor, toColor], [0.0, 1.0]);

    /// <summary>
    /// Creates a linear gradient scale from three colors
    /// </summary>
    public static ColorBlend ThreeColors(Color fromColor, Color middleColor, Color toColor) =>
        new([fromColor, middleColor, toColor], [0, 0.5, 1]);
}
