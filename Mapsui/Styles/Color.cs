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

    public static Color Transparent => new() { A = 0, R = 255, G = 255, B = 255 };  
    
    public static readonly Color AliceBlue = FromString("#F0F8FF");
    public static readonly Color AntiqueWhite = FromString("#FAEBD7");
    public static readonly Color Aqua = FromString("#00FFFF");
    public static readonly Color Aquamarine = FromString("#7FFFD4");
    public static readonly Color Azure = FromString("#F0FFFF");
    public static readonly Color Beige = FromString("#F5F5DC");
    public static readonly Color Bisque = FromString("#FFE4C4");
    public static readonly Color Black = FromString("#000000");
    public static readonly Color BlanchedAlmond = FromString("#FFEBCD");
    public static readonly Color Blue = FromString("#0000FF");
    public static readonly Color BlueViolet = FromString("#8A2BE2");
    public static readonly Color Brown = FromString("#A52A2A");
    public static readonly Color BurlyWood = FromString("#DEB887");
    public static readonly Color CadetBlue = FromString("#5F9EA0");
    public static readonly Color Chartreuse = FromString("#7FFF00");
    public static readonly Color Chocolate = FromString("#D2691E");
    public static readonly Color Coral = FromString("#FF7F50");
    public static readonly Color CornflowerBlue = FromString("#6495ED");
    public static readonly Color Cornsilk = FromString("#FFF8DC");
    public static readonly Color Crimson = FromString("#DC143C");
    public static readonly Color Cyan = FromString("#00FFFF");
    public static readonly Color DarkBlue = FromString("#00008B");
    public static readonly Color DarkCyan = FromString("#008B8B");
    public static readonly Color DarkGoldenRod = FromString("#B8860B");
    public static readonly Color DarkGray = FromString("#A9A9A9");
    public static readonly Color DarkGrey = FromString("#A9A9A9");
    public static readonly Color DarkGreen = FromString("#006400");
    public static readonly Color DarkKhaki = FromString("#BDB76B");
    public static readonly Color DarkMagenta = FromString("#8B008B");
    public static readonly Color DarkOliveGreen = FromString("#556B2F");
    public static readonly Color DarkOrange = FromString("#FF8C00");
    public static readonly Color DarkOrchid = FromString("#9932CC");
    public static readonly Color DarkRed = FromString("#8B0000");
    public static readonly Color DarkSalmon = FromString("#E9967A");
    public static readonly Color DarkSeaGreen = FromString("#8FBC8F");
    public static readonly Color DarkSlateBlue = FromString("#483D8B");
    public static readonly Color DarkSlateGray = FromString("#2F4F4F");
    public static readonly Color DarkSlateGrey = FromString("#2F4F4F");
    public static readonly Color DarkTurquoise = FromString("#00CED1");
    public static readonly Color DarkViolet = FromString("#9400D3");
    public static readonly Color DeepPink = FromString("#FF1493");
    public static readonly Color DeepSkyBlue = FromString("#00BFFF");
    public static readonly Color DimGray = FromString("#696969");
    public static readonly Color DimGrey = FromString("#696969");
    public static readonly Color DodgerBlue = FromString("#1E90FF");
    public static readonly Color FireBrick = FromString("#B22222");
    public static readonly Color FloralWhite = FromString("#FFFAF0");
    public static readonly Color ForestGreen = FromString("#228B22");
    public static readonly Color Fuchsia = FromString("#FF00FF");
    public static readonly Color Gainsboro = FromString("#DCDCDC");
    public static readonly Color GhostWhite = FromString("#F8F8FF");
    public static readonly Color Gold = FromString("#FFD700");
    public static readonly Color GoldenRod = FromString("#DAA520");
    public static readonly Color Gray = FromString("#808080");
    public static readonly Color Grey = FromString("#808080");
    public static readonly Color Green = FromString("#008000");
    public static readonly Color GreenYellow = FromString("#ADFF2F");
    public static readonly Color HoneyDew = FromString("#F0FFF0");
    public static readonly Color HotPink = FromString("#FF69B4");
    public static readonly Color IndianRed = FromString("#CD5C5C");
    public static readonly Color Indigo = FromString("#4B0082");
    public static readonly Color Ivory = FromString("#FFFFF0");
    public static readonly Color Khaki = FromString("#F0E68C");
    public static readonly Color Lavender = FromString("#E6E6FA");
    public static readonly Color LavenderBlush = FromString("#FFF0F5");
    public static readonly Color LawnGreen = FromString("#7CFC00");
    public static readonly Color LemonChiffon = FromString("#FFFACD");
    public static readonly Color LightBlue = FromString("#ADD8E6");
    public static readonly Color LightCoral = FromString("#F08080");
    public static readonly Color LightCyan = FromString("#E0FFFF");
    public static readonly Color LightGoldenRodYellow = FromString("#FAFAD2");
    public static readonly Color LightGray = FromString("#D3D3D3");
    public static readonly Color LightGrey = FromString("#D3D3D3");
    public static readonly Color LightGreen = FromString("#90EE90");
    public static readonly Color LightPink = FromString("#FFB6C1");
    public static readonly Color LightSalmon = FromString("#FFA07A");
    public static readonly Color LightSeaGreen = FromString("#20B2AA");
    public static readonly Color LightSkyBlue = FromString("#87CEFA");
    public static readonly Color LightSlateGray = FromString("#778899");
    public static readonly Color LightSlateGrey = FromString("#778899");
    public static readonly Color LightSteelBlue = FromString("#B0C4DE");
    public static readonly Color LightYellow = FromString("#FFFFE0");
    public static readonly Color Lime = FromString("#00FF00");
    public static readonly Color LimeGreen = FromString("#32CD32");
    public static readonly Color Linen = FromString("#FAF0E6");
    public static readonly Color Magenta = FromString("#FF00FF");
    public static readonly Color Maroon = FromString("#800000");
    public static readonly Color MediumAquaMarine = FromString("#66CDAA");
    public static readonly Color MediumBlue = FromString("#0000CD");
    public static readonly Color MediumOrchid = FromString("#BA55D3");
    public static readonly Color MediumPurple = FromString("#9370DB");
    public static readonly Color MediumSeaGreen = FromString("#3CB371");
    public static readonly Color MediumSlateBlue = FromString("#7B68EE");
    public static readonly Color MediumSpringGreen = FromString("#00FA9A");
    public static readonly Color MediumTurquoise = FromString("#48D1CC");
    public static readonly Color MediumVioletRed = FromString("#C71585");
    public static readonly Color MidnightBlue = FromString("#191970");
    public static readonly Color MintCream = FromString("#F5FFFA");
    public static readonly Color MistyRose = FromString("#FFE4E1");
    public static readonly Color Moccasin = FromString("#FFE4B5");
    public static readonly Color NavajoWhite = FromString("#FFDEAD");
    public static readonly Color Navy = FromString("#000080");
    public static readonly Color OldLace = FromString("#FDF5E6");
    public static readonly Color Olive = FromString("#808000");
    public static readonly Color OliveDrab = FromString("#6B8E23");
    public static readonly Color Orange = FromString("#FFA500");
    public static readonly Color OrangeRed = FromString("#FF4500");
    public static readonly Color Orchid = FromString("#DA70D6");
    public static readonly Color PaleGoldenRod = FromString("#EEE8AA");
    public static readonly Color PaleGreen = FromString("#98FB98");
    public static readonly Color PaleTurquoise = FromString("#AFEEEE");
    public static readonly Color PaleVioletRed = FromString("#DB7093");
    public static readonly Color PapayaWhip = FromString("#FFEFD5");
    public static readonly Color PeachPuff = FromString("#FFDAB9");
    public static readonly Color Peru = FromString("#CD853F");
    public static readonly Color Pink = FromString("#FFC0CB");
    public static readonly Color Plum = FromString("#DDA0DD");
    public static readonly Color PowderBlue = FromString("#B0E0E6");
    public static readonly Color Purple = FromString("#800080");
    public static readonly Color RebeccaPurple = FromString("#663399");
    public static readonly Color Red = FromString("#FF0000");
    public static readonly Color RosyBrown = FromString("#BC8F8F");
    public static readonly Color RoyalBlue = FromString("#4169E1");
    public static readonly Color SaddleBrown = FromString("#8B4513");
    public static readonly Color Salmon = FromString("#FA8072");
    public static readonly Color SandyBrown = FromString("#F4A460");
    public static readonly Color SeaGreen = FromString("#2E8B57");
    public static readonly Color SeaShell = FromString("#FFF5EE");
    public static readonly Color Sienna = FromString("#A0522D");
    public static readonly Color Silver = FromString("#C0C0C0");
    public static readonly Color SkyBlue = FromString("#87CEEB");
    public static readonly Color SlateBlue = FromString("#6A5ACD");
    public static readonly Color SlateGray = FromString("#708090");
    public static readonly Color SlateGrey = FromString("#708090");
    public static readonly Color Snow = FromString("#FFFAFA");
    public static readonly Color SpringGreen = FromString("#00FF7F");
    public static readonly Color SteelBlue = FromString("#4682B4");
    public static readonly Color Tan = FromString("#D2B48C");
    public static readonly Color Teal = FromString("#008080");
    public static readonly Color Thistle = FromString("#D8BFD8");
    public static readonly Color Tomato = FromString("#FF6347");
    public static readonly Color Turquoise = FromString("#40E0D0");
    public static readonly Color Violet = FromString("#EE82EE");
    public static readonly Color Wheat = FromString("#F5DEB3");
    public static readonly Color White = FromString("#FFFFFF");
    public static readonly Color WhiteSmoke = FromString("#F5F5F5");
    public static readonly Color Yellow = FromString("#FFFF00");
    public static readonly Color YellowGreen = FromString("#9ACD32");

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
}
