// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mapsui.Styles;

public class Color
{
    public Color()
    {
        A = 255;
    }

    public Color(Color color)
    {
        R = color.R;
        G = color.G;
        B = color.B;
        A = color.A;
    }

    public Color(int red, int green, int blue, int alpha = 255)
    {
        R = red;
        G = green;
        B = blue;
        A = alpha;
    }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public int A { get; set; }

    public static Color Transparent => new() { A = 0, R = 255, G = 255, B = 255 };
    public static Color Black => new() { A = 255, R = 0, G = 0, B = 0 };
    public static Color White => new() { A = 255, R = 255, G = 255, B = 255 };
    public static Color DarkGray => FromString(KnownColors["darkgray"]);
    public static Color Gray => new() { A = 255, R = 128, G = 128, B = 128 };
    public static Color Red => new() { A = 255, R = 255, G = 0, B = 0 };
    public static Color Yellow => new() { A = 255, R = 255, G = 255, B = 0 };
    public static Color Green => new() { A = 255, R = 0, G = 128, B = 0 };
    public static Color Cyan => new() { A = 255, R = 0, G = 255, B = 255 };
    public static Color Blue => new() { A = 255, R = 0, G = 0, B = 255 };
    public static Color Orange => new() { A = 255, R = 255, G = 165, B = 0 };
    public static Color Indigo => new() { A = 255, R = 75, G = 0, B = 130 };
    public static Color Violet => new() { A = 255, R = 238, G = 130, B = 238 };

    public static Color FromArgb(int a, int r, int g, int b)
    {
        return new Color { A = a, R = r, G = g, B = b };
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is Color color))
            return false;
        return Equals(color);
    }

    public bool Equals(Color? color)
    {
        if (color == null)
            return false;

        if (R != color.R) return false;
        if (G != color.G) return false;
        if (B != color.B) return false;
        if (A != color.A) return false;
        return true;
    }

    public override int GetHashCode()
    {
        return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
    }

    public static bool operator ==(Color? color1, Color? color2)
    {
        return Equals(color1, color2);
    }

    public static bool operator !=(Color? color1, Color? color2)
    {
        return !Equals(color1, color2);
    }

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
        Color? result = null;

        from = from.Trim().ToLower();

        // Check, if it is a known color
        if (KnownColors.ContainsKey(from))
            from = KnownColors[from];

        if (from.StartsWith("#"))
        {
            if (from.Length == 7)
            {
                var color = int.Parse(from.Substring(1), NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture);
                result = new Color(color >> 16 & 0xFF, color >> 8 & 0xFF, color & 0xFF);
            }
            else if (from.Length == 4)
            {
                var color = int.Parse(from.Substring(1), NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture);
                var r = (color >> 8 & 0xF) * 16 + (color >> 8 & 0xF);
                var g = (color >> 4 & 0xF) * 16 + (color >> 4 & 0xF);
                var b = (color & 0xF) * 16 + (color & 0xF);
                result = new Color(r, g, b);
            }
        }
        else if (from.StartsWith("rgba"))
        {
            var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

            if (split.Length != 4)
                throw new ArgumentException($"color {from} isn't a valid color");

            var r = int.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var g = int.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
            var b = int.Parse(split[2].Trim(), CultureInfo.InvariantCulture);
            var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

            result = new Color(r, g, b, (int)(a * 255));
        }
        else if (from.StartsWith("rgb"))
        {
            var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

            if (split.Length != 3)
                throw new ArgumentException($"color {from} isn't a valid color");

            var r = int.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var g = int.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
            var b = int.Parse(split[2].Trim(), CultureInfo.InvariantCulture);

            result = new Color(r, g, b);
        }
        else if (from.StartsWith("hsla"))
        {
            var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

            if (split.Length != 4)
                throw new ArgumentException($"color {from} isn't a valid color");

            var h = float.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var s = float.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
            var l = float.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
            var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

            result = FromHsl(h / 360.0f, s / 100.0f, l / 100.0f, (int)(a * 255));
        }
        else if (from.StartsWith("hsl"))
        {
            var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

            if (split.Length != 3)
                throw new ArgumentException($"color {from} isn't a valid color");

            var h = float.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
            var s = float.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
            var l = float.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);

            result = FromHsl(h / 360.0f, s / 100.0f, l / 100.0f);
        }

        if (result is null)
        {
            throw new ArgumentException($"Could not create color from input string '{from}'");
        }
        return result;
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

    /// <summary>
    /// Known HTML color names and hex code for RGB color
    /// </summary>
    public static readonly Dictionary<string, string> KnownColors = new()
    {
        { "AliceBlue".ToLower(), "#F0F8FF" },
        { "AntiqueWhite".ToLower(), "#FAEBD7" },
        { "Aqua".ToLower(), "#00FFFF" },
        { "Aquamarine".ToLower(), "#7FFFD4" },
        { "Azure".ToLower(), "#F0FFFF" },
        { "Beige".ToLower(), "#F5F5DC" },
        { "Bisque".ToLower(), "#FFE4C4" },
        { "Black".ToLower(), "#000000" },
        { "BlanchedAlmond".ToLower(), "#FFEBCD" },
        { "Blue".ToLower(), "#0000FF" },
        { "BlueViolet".ToLower(), "#8A2BE2" },
        { "Brown".ToLower(), "#A52A2A" },
        { "BurlyWood".ToLower(), "#DEB887" },
        { "CadetBlue".ToLower(), "#5F9EA0" },
        { "Chartreuse".ToLower(), "#7FFF00" },
        { "Chocolate".ToLower(), "#D2691E" },
        { "Coral".ToLower(), "#FF7F50" },
        { "CornflowerBlue".ToLower(), "#6495ED" },
        { "Cornsilk".ToLower(), "#FFF8DC" },
        { "Crimson".ToLower(), "#DC143C" },
        { "Cyan".ToLower(), "#00FFFF" },
        { "DarkBlue".ToLower(), "#00008B" },
        { "DarkCyan".ToLower(), "#008B8B" },
        { "DarkGoldenRod".ToLower(), "#B8860B" },
        { "DarkGray".ToLower(), "#A9A9A9" },
        { "DarkGrey".ToLower(), "#A9A9A9" },
        { "DarkGreen".ToLower(), "#006400" },
        { "DarkKhaki".ToLower(), "#BDB76B" },
        { "DarkMagenta".ToLower(), "#8B008B" },
        { "DarkOliveGreen".ToLower(), "#556B2F" },
        { "DarkOrange".ToLower(), "#FF8C00" },
        { "DarkOrchid".ToLower(), "#9932CC" },
        { "DarkRed".ToLower(), "#8B0000" },
        { "DarkSalmon".ToLower(), "#E9967A" },
        { "DarkSeaGreen".ToLower(), "#8FBC8F" },
        { "DarkSlateBlue".ToLower(), "#483D8B" },
        { "DarkSlateGray".ToLower(), "#2F4F4F" },
        { "DarkSlateGrey".ToLower(), "#2F4F4F" },
        { "DarkTurquoise".ToLower(), "#00CED1" },
        { "DarkViolet".ToLower(), "#9400D3" },
        { "DeepPink".ToLower(), "#FF1493" },
        { "DeepSkyBlue".ToLower(), "#00BFFF" },
        { "DimGray".ToLower(), "#696969" },
        { "DimGrey".ToLower(), "#696969" },
        { "DodgerBlue".ToLower(), "#1E90FF" },
        { "FireBrick".ToLower(), "#B22222" },
        { "FloralWhite".ToLower(), "#FFFAF0" },
        { "ForestGreen".ToLower(), "#228B22" },
        { "Fuchsia".ToLower(), "#FF00FF" },
        { "Gainsboro".ToLower(), "#DCDCDC" },
        { "GhostWhite".ToLower(), "#F8F8FF" },
        { "Gold".ToLower(), "#FFD700" },
        { "GoldenRod".ToLower(), "#DAA520" },
        { "Gray".ToLower(), "#808080" },
        { "Grey".ToLower(), "#808080" },
        { "Green".ToLower(), "#008000" },
        { "GreenYellow".ToLower(), "#ADFF2F" },
        { "HoneyDew".ToLower(), "#F0FFF0" },
        { "HotPink".ToLower(), "#FF69B4" },
        { "IndianRed ".ToLower(), "#CD5C5C" },
        { "Indigo ".ToLower(), "#4B0082" },
        { "Ivory".ToLower(), "#FFFFF0" },
        { "Khaki".ToLower(), "#F0E68C" },
        { "Lavender".ToLower(), "#E6E6FA" },
        { "LavenderBlush".ToLower(), "#FFF0F5" },
        { "LawnGreen".ToLower(), "#7CFC00" },
        { "LemonChiffon".ToLower(), "#FFFACD" },
        { "LightBlue".ToLower(), "#ADD8E6" },
        { "LightCoral".ToLower(), "#F08080" },
        { "LightCyan".ToLower(), "#E0FFFF" },
        { "LightGoldenRodYellow".ToLower(), "#FAFAD2" },
        { "LightGray".ToLower(), "#D3D3D3" },
        { "LightGrey".ToLower(), "#D3D3D3" },
        { "LightGreen".ToLower(), "#90EE90" },
        { "LightPink".ToLower(), "#FFB6C1" },
        { "LightSalmon".ToLower(), "#FFA07A" },
        { "LightSeaGreen".ToLower(), "#20B2AA" },
        { "LightSkyBlue".ToLower(), "#87CEFA" },
        { "LightSlateGray".ToLower(), "#778899" },
        { "LightSlateGrey".ToLower(), "#778899" },
        { "LightSteelBlue".ToLower(), "#B0C4DE" },
        { "LightYellow".ToLower(), "#FFFFE0" },
        { "Lime".ToLower(), "#00FF00" },
        { "LimeGreen".ToLower(), "#32CD32" },
        { "Linen".ToLower(), "#FAF0E6" },
        { "Magenta".ToLower(), "#FF00FF" },
        { "Maroon".ToLower(), "#800000" },
        { "MediumAquaMarine".ToLower(), "#66CDAA" },
        { "MediumBlue".ToLower(), "#0000CD" },
        { "MediumOrchid".ToLower(), "#BA55D3" },
        { "MediumPurple".ToLower(), "#9370DB" },
        { "MediumSeaGreen".ToLower(), "#3CB371" },
        { "MediumSlateBlue".ToLower(), "#7B68EE" },
        { "MediumSpringGreen".ToLower(), "#00FA9A" },
        { "MediumTurquoise".ToLower(), "#48D1CC" },
        { "MediumVioletRed".ToLower(), "#C71585" },
        { "MidnightBlue".ToLower(), "#191970" },
        { "MintCream".ToLower(), "#F5FFFA" },
        { "MistyRose".ToLower(), "#FFE4E1" },
        { "Moccasin".ToLower(), "#FFE4B5" },
        { "NavajoWhite".ToLower(), "#FFDEAD" },
        { "Navy".ToLower(), "#000080" },
        { "OldLace".ToLower(), "#FDF5E6" },
        { "Olive".ToLower(), "#808000" },
        { "OliveDrab".ToLower(), "#6B8E23" },
        { "Orange".ToLower(), "#FFA500" },
        { "OrangeRed".ToLower(), "#FF4500" },
        { "Orchid".ToLower(), "#DA70D6" },
        { "PaleGoldenRod".ToLower(), "#EEE8AA" },
        { "PaleGreen".ToLower(), "#98FB98" },
        { "PaleTurquoise".ToLower(), "#AFEEEE" },
        { "PaleVioletRed".ToLower(), "#DB7093" },
        { "PapayaWhip".ToLower(), "#FFEFD5" },
        { "PeachPuff".ToLower(), "#FFDAB9" },
        { "Peru".ToLower(), "#CD853F" },
        { "Pink".ToLower(), "#FFC0CB" },
        { "Plum".ToLower(), "#DDA0DD" },
        { "PowderBlue".ToLower(), "#B0E0E6" },
        { "Purple".ToLower(), "#800080" },
        { "RebeccaPurple".ToLower(), "#663399" },
        { "Red".ToLower(), "#FF0000" },
        { "RosyBrown".ToLower(), "#BC8F8F" },
        { "RoyalBlue".ToLower(), "#4169E1" },
        { "SaddleBrown".ToLower(), "#8B4513" },
        { "Salmon".ToLower(), "#FA8072" },
        { "SandyBrown".ToLower(), "#F4A460" },
        { "SeaGreen".ToLower(), "#2E8B57" },
        { "SeaShell".ToLower(), "#FFF5EE" },
        { "Sienna".ToLower(), "#A0522D" },
        { "Silver".ToLower(), "#C0C0C0" },
        { "SkyBlue".ToLower(), "#87CEEB" },
        { "SlateBlue".ToLower(), "#6A5ACD" },
        { "SlateGray".ToLower(), "#708090" },
        { "SlateGrey".ToLower(), "#708090" },
        { "Snow".ToLower(), "#FFFAFA" },
        { "SpringGreen".ToLower(), "#00FF7F" },
        { "SteelBlue".ToLower(), "#4682B4" },
        { "Tan".ToLower(), "#D2B48C" },
        { "Teal".ToLower(), "#008080" },
        { "Thistle".ToLower(), "#D8BFD8" },
        { "Tomato".ToLower(), "#FF6347" },
        { "Turquoise".ToLower(), "#40E0D0" },
        { "Violet".ToLower(), "#EE82EE" },
        { "Wheat".ToLower(), "#F5DEB3" },
        { "White".ToLower(), "#FFFFFF" },
        { "WhiteSmoke".ToLower(), "#F5F5F5" },
        { "Yellow".ToLower(), "#FFFF00" },
        { "YellowGreen".ToLower(), "#9ACD32" }
    };
}
