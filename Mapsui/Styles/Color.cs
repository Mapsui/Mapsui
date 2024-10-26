using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mapsui.Styles;

public readonly record struct Color
{
    private static Dictionary<string, Color>? _namedColors;

    public Color() { }

    public Color(Color color) : this(color.R, color.G, color.B, color.A) { }

    public Color(int red, int green, int blue, int alpha = 255)
    {
        R = red;
        G = green;
        B = blue;
        A = alpha;
    }

    private static Dictionary<string, Color> NamedColors
    {
        get
        {
            return _namedColors ??= new(StringComparer.InvariantCultureIgnoreCase)
            {
                { nameof(Color.AliceBlue), Color.AliceBlue },
                { nameof(Color.AntiqueWhite), Color.AntiqueWhite },
                { nameof(Color.Aqua), Color.Aqua },
                { nameof(Color.Aquamarine), Color.Aquamarine },
                { nameof(Color.Azure), Color.Azure },
                { nameof(Color.Beige), Color.Beige },
                { nameof(Color.Bisque), Color.Bisque },
                { nameof(Color.Black), Color.Black },
                { nameof(Color.BlanchedAlmond), Color.BlanchedAlmond },
                { nameof(Color.Blue), Color.Blue },
                { nameof(Color.BlueViolet), Color.BlueViolet },
                { nameof(Color.Brown), Color.Brown },
                { nameof(Color.BurlyWood), Color.BurlyWood },
                { nameof(Color.CadetBlue), Color.CadetBlue },
                { nameof(Color.Chartreuse), Color.Chartreuse },
                { nameof(Color.Chocolate), Color.Chocolate },
                { nameof(Color.Coral), Color.Coral },
                { nameof(Color.CornflowerBlue), Color.CornflowerBlue },
                { nameof(Color.Cornsilk), Color.Cornsilk },
                { nameof(Color.Crimson), Color.Crimson },
                { nameof(Color.Cyan), Color.Cyan },
                { nameof(Color.DarkBlue), Color.DarkBlue },
                { nameof(Color.DarkCyan), Color.DarkCyan },
                { nameof(Color.DarkGoldenRod), Color.DarkGoldenRod },
                { nameof(Color.DarkGray), Color.DarkGray },
                { nameof(Color.DarkGreen), Color.DarkGreen },
                { nameof(Color.DarkKhaki), Color.DarkKhaki },
                { nameof(Color.DarkMagenta), Color.DarkMagenta },
                { nameof(Color.DarkOliveGreen), Color.DarkOliveGreen },
                { nameof(Color.DarkOrange), Color.DarkOrange },
                { nameof(Color.DarkOrchid), Color.DarkOrchid },
                { nameof(Color.DarkRed), Color.DarkRed },
                { nameof(Color.DarkSalmon), Color.DarkSalmon },
                { nameof(Color.DarkSeaGreen), Color.DarkSeaGreen },
                { nameof(Color.DarkSlateBlue), Color.DarkSlateBlue },
                { nameof(Color.DarkSlateGray), Color.DarkSlateGray },
                { nameof(Color.DarkTurquoise), Color.DarkTurquoise },
                { nameof(Color.DarkViolet), Color.DarkViolet },
                { nameof(Color.DeepPink), Color.DeepPink },
                { nameof(Color.DeepSkyBlue), Color.DeepSkyBlue },
                { nameof(Color.DimGray), Color.DimGray },
                { nameof(Color.DodgerBlue), Color.DodgerBlue },
                { nameof(Color.FireBrick), Color.FireBrick },
                { nameof(Color.FloralWhite), Color.FloralWhite },
                { nameof(Color.ForestGreen), Color.ForestGreen },
                { nameof(Color.Fuchsia), Color.Fuchsia },
                { nameof(Color.Gainsboro), Color.Gainsboro },
                { nameof(Color.GhostWhite), Color.GhostWhite },
                { nameof(Color.Gold), Color.Gold },
                { nameof(Color.GoldenRod), Color.GoldenRod },
                { nameof(Color.Gray), Color.Gray },
                { nameof(Color.Green), Color.Green },
                { nameof(Color.GreenYellow), Color.GreenYellow },
                { nameof(Color.HoneyDew), Color.HoneyDew },
                { nameof(Color.HotPink), Color.HotPink },
                { nameof(Color.IndianRed), Color.IndianRed },
                { nameof(Color.Indigo), Color.Indigo },
                { nameof(Color.Ivory), Color.Ivory },
                { nameof(Color.Khaki), Color.Khaki },
                { nameof(Color.Lavender), Color.Lavender },
                { nameof(Color.LavenderBlush), Color.LavenderBlush },
                { nameof(Color.LawnGreen), Color.LawnGreen },
                { nameof(Color.LemonChiffon), Color.LemonChiffon },
                { nameof(Color.LightBlue), Color.LightBlue },
                { nameof(Color.LightCoral), Color.LightCoral },
                { nameof(Color.LightCyan), Color.LightCyan },
                { nameof(Color.LightGoldenRodYellow), Color.LightGoldenRodYellow },
                { nameof(Color.LightGray), Color.LightGray },
                { nameof(Color.LightGreen), Color.LightGreen },
                { nameof(Color.LightPink), Color.LightPink },
                { nameof(Color.LightSalmon), Color.LightSalmon },
                { nameof(Color.LightSeaGreen), Color.LightSeaGreen },
                { nameof(Color.LightSkyBlue), Color.LightSkyBlue },
                { nameof(Color.LightSlateGray), Color.LightSlateGray },
                { nameof(Color.LightSteelBlue), Color.LightSteelBlue },
                { nameof(Color.LightYellow), Color.LightYellow },
                { nameof(Color.Lime), Color.Lime },
                { nameof(Color.LimeGreen), Color.LimeGreen },
                { nameof(Color.Linen), Color.Linen },
                { nameof(Color.Magenta), Color.Magenta },
                { nameof(Color.Maroon), Color.Maroon },
                { nameof(Color.MediumAquaMarine), Color.MediumAquaMarine },
                { nameof(Color.MediumBlue), Color.MediumBlue },
                { nameof(Color.MediumOrchid), Color.MediumOrchid },
                { nameof(Color.MediumPurple), Color.MediumPurple },
                { nameof(Color.MediumSeaGreen), Color.MediumSeaGreen },
                { nameof(Color.MediumSlateBlue), Color.MediumSlateBlue },
                { nameof(Color.MediumSpringGreen), Color.MediumSpringGreen },
                { nameof(Color.MediumTurquoise), Color.MediumTurquoise },
                { nameof(Color.MediumVioletRed), Color.MediumVioletRed },
                { nameof(Color.MidnightBlue), Color.MidnightBlue },
                { nameof(Color.MintCream), Color.MintCream },
                { nameof(Color.MistyRose), Color.MistyRose },
                { nameof(Color.Moccasin), Color.Moccasin },
                { nameof(Color.NavajoWhite), Color.NavajoWhite },
                { nameof(Color.Navy), Color.Navy },
                { nameof(Color.OldLace), Color.OldLace },
                { nameof(Color.Olive), Color.Olive },
                { nameof(Color.OliveDrab), Color.OliveDrab },
                { nameof(Color.Orange), Color.Orange },
                { nameof(Color.OrangeRed), Color.OrangeRed },
                { nameof(Color.Orchid), Color.Orchid },
                { nameof(Color.PaleGoldenRod), Color.PaleGoldenRod },
                { nameof(Color.PaleGreen), Color.PaleGreen },
                { nameof(Color.PaleTurquoise), Color.PaleTurquoise },
                { nameof(Color.PaleVioletRed), Color.PaleVioletRed },
                { nameof(Color.PapayaWhip), Color.PapayaWhip },
                { nameof(Color.PeachPuff), Color.PeachPuff },
                { nameof(Color.Peru), Color.Peru },
                { nameof(Color.Pink), Color.Pink },
                { nameof(Color.Plum), Color.Plum },
                { nameof(Color.PowderBlue), Color.PowderBlue },
                { nameof(Color.Purple), Color.Purple },
                { nameof(Color.Red), Color.Red },
                { nameof(Color.RosyBrown), Color.RosyBrown },
                { nameof(Color.RoyalBlue), Color.RoyalBlue },
                { nameof(Color.SaddleBrown), Color.SaddleBrown },
                { nameof(Color.Salmon), Color.Salmon },
                { nameof(Color.SandyBrown), Color.SandyBrown },
                { nameof(Color.SeaGreen), Color.SeaGreen },
                { nameof(Color.SeaShell), Color.SeaShell },
                { nameof(Color.Sienna), Color.Sienna },
                { nameof(Color.Silver), Color.Silver },
                { nameof(Color.SkyBlue), Color.SkyBlue },
                { nameof(Color.SlateBlue), Color.SlateBlue },
                { nameof(Color.SlateGray), Color.SlateGray },
                { nameof(Color.Snow), Color.Snow },
                { nameof(Color.SpringGreen), Color.SpringGreen },
                { nameof(Color.SteelBlue), Color.SteelBlue },
                { nameof(Color.Tan), Color.Tan },
                { nameof(Color.Teal), Color.Teal },
                { nameof(Color.Thistle), Color.Thistle },
                { nameof(Color.Tomato), Color.Tomato },
                { nameof(Color.Turquoise), Color.Turquoise },
                { nameof(Color.Violet), Color.Violet },
                { nameof(Color.Wheat), Color.Wheat },
                { nameof(Color.White), Color.White },
                { nameof(Color.WhiteSmoke), Color.WhiteSmoke },
                { nameof(Color.Yellow), Color.Yellow },
                { nameof(Color.YellowGreen), Color.YellowGreen }
            };
        }
    }

    public int R { get; init; }
    public int G { get; init; }
    public int B { get; init; }
    public int A { get; init; } = 255;

    public static Color Transparent { get; } = new() { A = 0, R = 255, G = 255, B = 255 };

    public static Color AliceBlue { get; } = FromString("#F0F8FF");
    public static Color AntiqueWhite { get; } = FromString("#FAEBD7");
    public static Color Aqua { get; } = FromString("#00FFFF");
    public static Color Aquamarine { get; } = FromString("#7FFFD4");
    public static Color Azure { get; } = FromString("#F0FFFF");
    public static Color Beige { get; } = FromString("#F5F5DC");
    public static Color Bisque { get; } = FromString("#FFE4C4");
    public static Color Black { get; } = FromString("#000000");
    public static Color BlanchedAlmond { get; } = FromString("#FFEBCD");
    public static Color Blue { get; } = FromString("#0000FF");
    public static Color BlueViolet { get; } = FromString("#8A2BE2");
    public static Color Brown { get; } = FromString("#A52A2A");
    public static Color BurlyWood { get; } = FromString("#DEB887");
    public static Color CadetBlue { get; } = FromString("#5F9EA0");
    public static Color Chartreuse { get; } = FromString("#7FFF00");
    public static Color Chocolate { get; } = FromString("#D2691E");
    public static Color Coral { get; } = FromString("#FF7F50");
    public static Color CornflowerBlue { get; } = FromString("#6495ED");
    public static Color Cornsilk { get; } = FromString("#FFF8DC");
    public static Color Crimson { get; } = FromString("#DC143C");
    public static Color Cyan { get; } = FromString("#00FFFF");
    public static Color DarkBlue { get; } = FromString("#00008B");
    public static Color DarkCyan { get; } = FromString("#008B8B");
    public static Color DarkGoldenRod { get; } = FromString("#B8860B");
    public static Color DarkGray { get; } = FromString("#A9A9A9");
    public static Color DarkGrey { get; } = FromString("#A9A9A9");
    public static Color DarkGreen { get; } = FromString("#006400");
    public static Color DarkKhaki { get; } = FromString("#BDB76B");
    public static Color DarkMagenta { get; } = FromString("#8B008B");
    public static Color DarkOliveGreen { get; } = FromString("#556B2F");
    public static Color DarkOrange { get; } = FromString("#FF8C00");
    public static Color DarkOrchid { get; } = FromString("#9932CC");
    public static Color DarkRed { get; } = FromString("#8B0000");
    public static Color DarkSalmon { get; } = FromString("#E9967A");
    public static Color DarkSeaGreen { get; } = FromString("#8FBC8F");
    public static Color DarkSlateBlue { get; } = FromString("#483D8B");
    public static Color DarkSlateGray { get; } = FromString("#2F4F4F");
    public static Color DarkSlateGrey { get; } = FromString("#2F4F4F");
    public static Color DarkTurquoise { get; } = FromString("#00CED1");
    public static Color DarkViolet { get; } = FromString("#9400D3");
    public static Color DeepPink { get; } = FromString("#FF1493");
    public static Color DeepSkyBlue { get; } = FromString("#00BFFF");
    public static Color DimGray { get; } = FromString("#696969");
    public static Color DimGrey { get; } = FromString("#696969");
    public static Color DodgerBlue { get; } = FromString("#1E90FF");
    public static Color FireBrick { get; } = FromString("#B22222");
    public static Color FloralWhite { get; } = FromString("#FFFAF0");
    public static Color ForestGreen { get; } = FromString("#228B22");
    public static Color Fuchsia { get; } = FromString("#FF00FF");
    public static Color Gainsboro { get; } = FromString("#DCDCDC");
    public static Color GhostWhite { get; } = FromString("#F8F8FF");
    public static Color Gold { get; } = FromString("#FFD700");
    public static Color GoldenRod { get; } = FromString("#DAA520");
    public static Color Gray { get; } = FromString("#808080");
    public static Color Grey { get; } = FromString("#808080");
    public static Color Green { get; } = FromString("#008000");
    public static Color GreenYellow { get; } = FromString("#ADFF2F");
    public static Color HoneyDew { get; } = FromString("#F0FFF0");
    public static Color HotPink { get; } = FromString("#FF69B4");
    public static Color IndianRed { get; } = FromString("#CD5C5C");
    public static Color Indigo { get; } = FromString("#4B0082");
    public static Color Ivory { get; } = FromString("#FFFFF0");
    public static Color Khaki { get; } = FromString("#F0E68C");
    public static Color Lavender { get; } = FromString("#E6E6FA");
    public static Color LavenderBlush { get; } = FromString("#FFF0F5");
    public static Color LawnGreen { get; } = FromString("#7CFC00");
    public static Color LemonChiffon { get; } = FromString("#FFFACD");
    public static Color LightBlue { get; } = FromString("#ADD8E6");
    public static Color LightCoral { get; } = FromString("#F08080");
    public static Color LightCyan { get; } = FromString("#E0FFFF");
    public static Color LightGoldenRodYellow { get; } = FromString("#FAFAD2");
    public static Color LightGray { get; } = FromString("#D3D3D3");
    public static Color LightGrey { get; } = FromString("#D3D3D3");
    public static Color LightGreen { get; } = FromString("#90EE90");
    public static Color LightPink { get; } = FromString("#FFB6C1");
    public static Color LightSalmon { get; } = FromString("#FFA07A");
    public static Color LightSeaGreen { get; } = FromString("#20B2AA");
    public static Color LightSkyBlue { get; } = FromString("#87CEFA");
    public static Color LightSlateGray { get; } = FromString("#778899");
    public static Color LightSlateGrey { get; } = FromString("#778899");
    public static Color LightSteelBlue { get; } = FromString("#B0C4DE");
    public static Color LightYellow { get; } = FromString("#FFFFE0");
    public static Color Lime { get; } = FromString("#00FF00");
    public static Color LimeGreen { get; } = FromString("#32CD32");
    public static Color Linen { get; } = FromString("#FAF0E6");
    public static Color Magenta { get; } = FromString("#FF00FF");
    public static Color Maroon { get; } = FromString("#800000");
    public static Color MediumAquaMarine { get; } = FromString("#66CDAA");
    public static Color MediumBlue { get; } = FromString("#0000CD");
    public static Color MediumOrchid { get; } = FromString("#BA55D3");
    public static Color MediumPurple { get; } = FromString("#9370DB");
    public static Color MediumSeaGreen { get; } = FromString("#3CB371");
    public static Color MediumSlateBlue { get; } = FromString("#7B68EE");
    public static Color MediumSpringGreen { get; } = FromString("#00FA9A");
    public static Color MediumTurquoise { get; } = FromString("#48D1CC");
    public static Color MediumVioletRed { get; } = FromString("#C71585");
    public static Color MidnightBlue { get; } = FromString("#191970");
    public static Color MintCream { get; } = FromString("#F5FFFA");
    public static Color MistyRose { get; } = FromString("#FFE4E1");
    public static Color Moccasin { get; } = FromString("#FFE4B5");
    public static Color NavajoWhite { get; } = FromString("#FFDEAD");
    public static Color Navy { get; } = FromString("#000080");
    public static Color OldLace { get; } = FromString("#FDF5E6");
    public static Color Olive { get; } = FromString("#808000");
    public static Color OliveDrab { get; } = FromString("#6B8E23");
    public static Color Orange { get; } = FromString("#FFA500");
    public static Color OrangeRed { get; } = FromString("#FF4500");
    public static Color Orchid { get; } = FromString("#DA70D6");
    public static Color PaleGoldenRod { get; } = FromString("#EEE8AA");
    public static Color PaleGreen { get; } = FromString("#98FB98");
    public static Color PaleTurquoise { get; } = FromString("#AFEEEE");
    public static Color PaleVioletRed { get; } = FromString("#DB7093");
    public static Color PapayaWhip { get; } = FromString("#FFEFD5");
    public static Color PeachPuff { get; } = FromString("#FFDAB9");
    public static Color Peru { get; } = FromString("#CD853F");
    public static Color Pink { get; } = FromString("#FFC0CB");
    public static Color Plum { get; } = FromString("#DDA0DD");
    public static Color PowderBlue { get; } = FromString("#B0E0E6");
    public static Color Purple { get; } = FromString("#800080");
    public static Color RebeccaPurple { get; } = FromString("#663399");
    public static Color Red { get; } = FromString("#FF0000");
    public static Color RosyBrown { get; } = FromString("#BC8F8F");
    public static Color RoyalBlue { get; } = FromString("#4169E1");
    public static Color SaddleBrown { get; } = FromString("#8B4513");
    public static Color Salmon { get; } = FromString("#FA8072");
    public static Color SandyBrown { get; } = FromString("#F4A460");
    public static Color SeaGreen { get; } = FromString("#2E8B57");
    public static Color SeaShell { get; } = FromString("#FFF5EE");
    public static Color Sienna { get; } = FromString("#A0522D");
    public static Color Silver { get; } = FromString("#C0C0C0");
    public static Color SkyBlue { get; } = FromString("#87CEEB");
    public static Color SlateBlue { get; } = FromString("#6A5ACD");
    public static Color SlateGray { get; } = FromString("#708090");
    public static Color SlateGrey { get; } = FromString("#708090");
    public static Color Snow { get; } = FromString("#FFFAFA");
    public static Color SpringGreen { get; } = FromString("#00FF7F");
    public static Color SteelBlue { get; } = FromString("#4682B4");
    public static Color Tan { get; } = FromString("#D2B48C");
    public static Color Teal { get; } = FromString("#008080");
    public static Color Thistle { get; } = FromString("#D8BFD8");
    public static Color Tomato { get; } = FromString("#FF6347");
    public static Color Turquoise { get; } = FromString("#40E0D0");
    public static Color Violet { get; } = FromString("#EE82EE");
    public static Color Wheat { get; } = FromString("#F5DEB3");
    public static Color White { get; } = FromString("#FFFFFF");
    public static Color WhiteSmoke { get; } = FromString("#F5F5F5");
    public static Color Yellow { get; } = FromString("#FFFF00");
    public static Color YellowGreen { get; } = FromString("#9ACD32");

    public static Color FromArgb(int a, int r, int g, int b) => new() { A = a, R = r, G = g, B = b };

    public static Color FromRgba(int r, int g, int b, int a) => new() { R = r, G = g, B = b, A = a };

    public static explicit operator System.Drawing.Color(Color color)
        => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

    public static explicit operator Color(System.Drawing.Color color) => new(color.R, color.G, color.B, color.A);

    /// <summary>
    /// Converts a string in Mapbox GL format to a Mapsui Color
    /// 
    /// This function assumes, that alpha is a float in range from 0.0 to 1.0.
    /// It converts this float in Mapsui Color alpha without rounding.
    /// The following colors could be converted:
    /// - Named colors with known Html names 
    /// - Colors as Html color values with leading '#' and 6 or 3 numbers
    /// - Function rgb(r,g,b) with values for red, green and blue
    /// - Function rgba(r,g,b,a) with values for red, green, blue and alpha. Here alpha is between 0.0 and 1.0 like opacity.
    /// - Function hsl(h,s,l) with values hue (0.0 to 360.0), saturation (0.0% - 100.0%) and lightness (0.0% - 100.0%)
    /// - Function hsla(h,s,l,a) with values hue (0.0 to 360.0), saturation (0.0% - 100.0%), lightness (0.0% - 100.0%) and alpha. Here alpha is between 0.0 and 1.0 like opacity.
    /// </summary>
    /// <param name="from">String with HTML color representation or function like rgb() or hsl()</param>
    /// <returns>Converted Mapsui Color</returns>
    public static Color FromString(string from)
    {
        from = from.Trim().ToLower();

        if (from.StartsWith('#'))
        {
            if (from.Length == 7)
            {
                var color = int.Parse(from[1..], NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture);
                return new Color(color >> 16 & 0xFF, color >> 8 & 0xFF, color & 0xFF);
            }
            if (from.Length == 4)
            {
                var color = int.Parse(from.AsSpan(1), NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture);
                var r = (color >> 8 & 0xF) * 16 + (color >> 8 & 0xF);
                var g = (color >> 4 & 0xF) * 16 + (color >> 4 & 0xF);
                var b = (color & 0xF) * 16 + (color & 0xF);
                return new Color(r, g, b);
            }
            throw new ArgumentException("The color # format should contain 3 or 6 hex numbers");
        }
        else if (from.StartsWith("rgba"))
        {
            var split = from[(from.IndexOf('(') + 1)..].TrimEnd(')').Split(',');

            if (split.Length != 4)
                throw new ArgumentException($"color {from} isn't a valid color");

            var r = int.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var g = int.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
            var b = int.Parse(split[2].Trim(), CultureInfo.InvariantCulture);
            var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

            return new Color(r, g, b, (int)(a * 255));
        }
        else if (from.StartsWith("rgb"))
        {
            var split = from[(from.IndexOf('(') + 1)..].TrimEnd(')').Split(',');

            if (split.Length != 3)
                throw new ArgumentException($"color {from} isn't a valid color");

            var r = int.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var g = int.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
            var b = int.Parse(split[2].Trim(), CultureInfo.InvariantCulture);

            return new Color(r, g, b);
        }
        else if (from.StartsWith("hsla"))
        {
            var split = from[(from.IndexOf('(') + 1)..].TrimEnd(')').Split(',');

            if (split.Length != 4)
                throw new ArgumentException($"color {from} isn't a valid color");

            var h = float.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var s = float.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
            var l = float.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
            var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

            return FromHsl(h / 360.0f, s / 100.0f, l / 100.0f, (int)(a * 255));
        }
        else if (from.StartsWith("hsl"))
        {
            var split = from[(from.IndexOf('(') + 1)..].TrimEnd(')').Split(',');

            if (split.Length != 3)
                throw new ArgumentException($"color {from} isn't a valid color");

            var h = float.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var s = float.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
            var l = float.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);

            return FromHsl(h / 360.0f, s / 100.0f, l / 100.0f);
        }
        else if (NamedColors.TryGetValue(from, out var color))
        {
            return color;
        }

        throw new ArgumentException($"Color string did not have any of the known prefixes. Could not create color from input string '{from}'. " +
            $"For named colors please use the Color statics, like 'Color.WhiteSmoke'");
    }

    /// <summary>
    /// Found at http://james-ramsden.com/convert-from-hsl-to-rgb-colour-codes-in-c/
    /// </summary>
    /// <param name="h"></param>
    /// <param name="s"></param>
    /// <param name="l"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Color FromHsl(float h, float s, float l, int a = 255)
    {
        double r = 0, g = 0, b = 0;
        // != 0
        if (l > float.Epsilon)
        {
            // == 0
            if (s < float.Epsilon)
                r = g = b = l;
            else
            {
                float temp2;

                if (l < 0.5)
                    temp2 = l * (1.0f + s);
                else
                    temp2 = l + s - (l * s);

                var temp1 = 2.0f * l - temp2;

                r = GetColorComponent(temp1, temp2, h + 1.0f / 3.0f);
                g = GetColorComponent(temp1, temp2, h);
                b = GetColorComponent(temp1, temp2, h - 1.0f / 3.0f);
            }
        }
        return FromArgb(a,
            (int)Math.Round(r * 255.0f),
            (int)Math.Round(g * 255.0f),
            (int)Math.Round(b * 255.0f));

    }

    /// <summary>
    /// Helper function for FromHsl function
    /// </summary>
    /// <param name="temp1"></param>
    /// <param name="temp2"></param>
    /// <param name="temp3"></param>
    /// <returns></returns>
    private static double GetColorComponent(float temp1, float temp2, float temp3)
    {
        if (temp3 < 0.0f)
            temp3 += 1.0f;
        else if (temp3 > 1.0f)
            temp3 -= 1.0f;

        if (temp3 < 1.0f / 6.0f)
            return temp1 + (temp2 - temp1) * 6.0f * temp3;
        else if (temp3 < 0.5f)
            return temp2;
        else if (temp3 < 2.0f / 3.0f)
            return temp1 + ((temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f);
        else
            return temp1;
    }

    /// <summary>
    /// Change alpha channel from given color to respect opacity
    /// </summary>
    /// <param name="color">Mapsui Color to change</param>
    /// <param name="opacity">Opacity of the new color</param>
    /// <returns>New color respecting old alpha and new opacity</returns>
    public static Color Opacity(Color color, float? opacity)
    {
        if (opacity == null)
            return color;

        return new Color(color.R, color.G, color.B, (int)Math.Round(color.A * (float)opacity));
    }

    public override string ToString()
    {
        return $"rgba({R},{G},{B},{A})";
    }
}
