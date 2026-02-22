using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

internal static class KnownColors
{
    private const int SZeroChar = 48;
    private const int SALower = 97;
    private const int SAUpper = 65;
    private const string SContextColor = "ContextColor ";
    internal const string SContextColorNoSpace = "ContextColor";

    private static readonly Dictionary<string, SKColor> s_knownArgbColors;

    static KnownColors()
    {
        s_knownArgbColors = new Dictionary<string, SKColor>();
        foreach (var item in typeof(SKColors).GetFields().Where(x => x.FieldType == typeof(SKColor)))
        {
            var value = (SKColor)item.GetValue(null);
            var num = (uint)((value.Alpha << 24) | (value.Red << 16) | (value.Green << 8) | value.Blue);
            var key = $"#{num,8:X8}";
            s_knownArgbColors[key] = value;
        }
    }

    private static string MatchColor(string colorString, out bool isKnownColor, out bool isNumericColor, out bool isContextColor, out bool isScRgbColor)
    {
        var text = colorString.Trim();
        var flag = text.Length is 4 or 5 or 7 or 9;
        if (flag && text[0] == '#')
        {
            isNumericColor = true;
            isScRgbColor = false;
            isKnownColor = false;
            isContextColor = false;
            return text;
        }
        isNumericColor = false;
        if (text.StartsWith("sc#", StringComparison.Ordinal))
        {
            isScRgbColor = true;
            isKnownColor = false;
            isContextColor = false;
        }
        else
        {
            isScRgbColor = false;
        }
        if (text.StartsWith("ContextColor ", StringComparison.OrdinalIgnoreCase))
        {
            isContextColor = true;
            isScRgbColor = false;
            isKnownColor = false;
            return text;
        }
        isContextColor = false;
        isKnownColor = true;
        return text;
    }

    internal static SKColor ParseColor(string color)
    {
        var text = MatchColor(color, out var isKnownColor, out var isNumericColor, out var isContextColor, out var isScRgbColor);
        if (!isKnownColor && !isNumericColor && !isScRgbColor && !isContextColor)
            throw new FormatException("Bad colour format");
        if (isNumericColor)
            return ParseHexColor(text);
        return ColorStringToKnownColor(text);
    }

    private static int ParseHexChar(char c)
    {
        if (c >= '0' && c <= '9') return c - 48;
        if (c >= 'a' && c <= 'f') return c - 97 + 10;
        if (c >= 'A' && c <= 'F') return c - 65 + 10;
        throw new FormatException("Bad format");
    }

    private static SKColor ParseHexColor(string trimmedColor)
    {
        int num = 255, num2, num3, num4;
        if (trimmedColor.Length > 7)
        {
            num = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
            num2 = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
            num3 = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
            num4 = ParseHexChar(trimmedColor[7]) * 16 + ParseHexChar(trimmedColor[8]);
        }
        else if (trimmedColor.Length > 5)
        {
            num2 = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
            num3 = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
            num4 = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
        }
        else if (trimmedColor.Length > 4)
        {
            num = ParseHexChar(trimmedColor[1]);
            num += num * 16;
            num2 = ParseHexChar(trimmedColor[2]);
            num2 += num2 * 16;
            num3 = ParseHexChar(trimmedColor[3]);
            num3 += num3 * 16;
            num4 = ParseHexChar(trimmedColor[4]);
            num4 += num4 * 16;
        }
        else
        {
            num2 = ParseHexChar(trimmedColor[1]);
            num2 += num2 * 16;
            num3 = ParseHexChar(trimmedColor[2]);
            num3 += num3 * 16;
            num4 = ParseHexChar(trimmedColor[3]);
            num4 += num4 * 16;
        }
        return SKColorFactory.MakeColor((byte)num2, (byte)num3, (byte)num4, (byte)num, "ParseHexColor");
    }

    private static SKColor ColorStringToKnownColor(string colorString) =>
        SKColorFactory.LogColor(InternalColorStringToKnownColor(colorString), "ColorStringToKnownColor");

    private static SKColor InternalColorStringToKnownColor(string colorString)
    {
        if (colorString != null)
        {
            var text = colorString.ToUpper(CultureInfo.InvariantCulture);
            switch (text.Length)
            {
                case 3:
                    if (text.Equals("RED")) return SKColors.Red;
                    if (text.Equals("TAN")) return SKColors.Tan;
                    break;
                case 4:
                    switch (text[0])
                    {
                        case 'A': if (text.Equals("AQUA")) return SKColors.Aqua; break;
                        case 'B': if (text.Equals("BLUE")) return SKColors.Blue; break;
                        case 'C': if (text.Equals("CYAN")) return SKColors.Cyan; break;
                        case 'G':
                            if (text.Equals("GOLD")) return SKColors.Gold;
                            if (text.Equals("GRAY")) return SKColors.Gray;
                            break;
                        case 'L': if (text.Equals("LIME")) return SKColors.Lime; break;
                        case 'N': if (text.Equals("NAVY")) return SKColors.Navy; break;
                        case 'P':
                            if (text.Equals("PERU")) return SKColors.Peru;
                            if (text.Equals("PINK")) return SKColors.Pink;
                            if (text.Equals("PLUM")) return SKColors.Plum;
                            break;
                        case 'S': if (text.Equals("SNOW")) return SKColors.Snow; break;
                        case 'T': if (text.Equals("TEAL")) return SKColors.Teal; break;
                    }
                    break;
                case 5:
                    switch (text[0])
                    {
                        case 'A': if (text.Equals("AZURE")) return SKColors.Azure; break;
                        case 'B':
                            if (text.Equals("BEIGE")) return SKColors.Beige;
                            if (text.Equals("BLACK")) return SKColors.Black;
                            if (text.Equals("BROWN")) return SKColors.Brown;
                            break;
                        case 'C': if (text.Equals("CORAL")) return SKColors.Coral; break;
                        case 'G': if (text.Equals("GREEN")) return SKColors.Green; break;
                        case 'I': if (text.Equals("IVORY")) return SKColors.Ivory; break;
                        case 'K': if (text.Equals("KHAKI")) return SKColors.Khaki; break;
                        case 'L': if (text.Equals("LINEN")) return SKColors.Linen; break;
                        case 'O': if (text.Equals("OLIVE")) return SKColors.Olive; break;
                        case 'W':
                            if (text.Equals("WHEAT")) return SKColors.Wheat;
                            if (text.Equals("WHITE")) return SKColors.White;
                            break;
                    }
                    break;
                case 6:
                    switch (text[0])
                    {
                        case 'B': if (text.Equals("BISQUE")) return SKColors.Bisque; break;
                        case 'I': if (text.Equals("INDIGO")) return SKColors.Indigo; break;
                        case 'M': if (text.Equals("MAROON")) return SKColors.Maroon; break;
                        case 'O':
                            if (text.Equals("ORANGE")) return SKColors.Orange;
                            if (text.Equals("ORCHID")) return SKColors.Orchid;
                            break;
                        case 'P': if (text.Equals("PURPLE")) return SKColors.Purple; break;
                        case 'S':
                            if (text.Equals("SALMON")) return SKColors.Salmon;
                            if (text.Equals("SIENNA")) return SKColors.Sienna;
                            if (text.Equals("SILVER")) return SKColors.Silver;
                            break;
                        case 'T': if (text.Equals("TOMATO")) return SKColors.Tomato; break;
                        case 'V': if (text.Equals("VIOLET")) return SKColors.Violet; break;
                        case 'Y': if (text.Equals("YELLOW")) return SKColors.Yellow; break;
                    }
                    break;
                case 7:
                    switch (text[0])
                    {
                        case 'C': if (text.Equals("CRIMSON")) return SKColors.Crimson; break;
                        case 'D':
                            if (text.Equals("DARKRED")) return SKColors.DarkRed;
                            if (text.Equals("DIMGRAY")) return SKColors.DimGray;
                            break;
                        case 'F': if (text.Equals("FUCHSIA")) return SKColors.Fuchsia; break;
                        case 'H': if (text.Equals("HOTPINK")) return SKColors.HotPink; break;
                        case 'M': if (text.Equals("MAGENTA")) return SKColors.Magenta; break;
                        case 'O': if (text.Equals("OLDLACE")) return SKColors.OldLace; break;
                        case 'S': if (text.Equals("SKYBLUE")) return SKColors.SkyBlue; break;
                        case 'T': if (text.Equals("THISTLE")) return SKColors.Thistle; break;
                    }
                    break;
                case 8:
                    switch (text[0])
                    {
                        case 'C': if (text.Equals("CORNSILK")) return SKColors.Cornsilk; break;
                        case 'D':
                            if (text.Equals("DARKBLUE")) return SKColors.DarkBlue;
                            if (text.Equals("DARKCYAN")) return SKColors.DarkCyan;
                            if (text.Equals("DARKGRAY")) return SKColors.DarkGray;
                            if (text.Equals("DEEPPINK")) return SKColors.DeepPink;
                            break;
                        case 'H': if (text.Equals("HONEYDEW")) return SKColors.Honeydew; break;
                        case 'L': if (text.Equals("LAVENDER")) return SKColors.Lavender; break;
                        case 'M': if (text.Equals("MOCCASIN")) return SKColors.Moccasin; break;
                        case 'S':
                            if (text.Equals("SEAGREEN")) return SKColors.SeaGreen;
                            if (text.Equals("SEASHELL")) return SKColors.SeaShell;
                            break;
                    }
                    break;
                case 9:
                    switch (text[0])
                    {
                        case 'A': if (text.Equals("ALICEBLUE")) return SKColors.AliceBlue; break;
                        case 'B': if (text.Equals("BURLYWOOD")) return SKColors.BurlyWood; break;
                        case 'C':
                            if (text.Equals("CADETBLUE")) return SKColors.CadetBlue;
                            if (text.Equals("CHOCOLATE")) return SKColors.Chocolate;
                            break;
                        case 'D':
                            if (text.Equals("DARKGREEN")) return SKColors.DarkGreen;
                            if (text.Equals("DARKKHAKI")) return SKColors.DarkKhaki;
                            break;
                        case 'F': if (text.Equals("FIREBRICK")) return SKColors.Firebrick; break;
                        case 'G':
                            if (text.Equals("GAINSBORO")) return SKColors.Gainsboro;
                            if (text.Equals("GOLDENROD")) return SKColors.Goldenrod;
                            break;
                        case 'I': if (text.Equals("INDIANRED")) return SKColors.IndianRed; break;
                        case 'L':
                            if (text.Equals("LAWNGREEN")) return SKColors.LawnGreen;
                            if (text.Equals("LIGHTBLUE")) return SKColors.LightBlue;
                            if (text.Equals("LIGHTCYAN")) return SKColors.LightCyan;
                            if (text.Equals("LIGHTGRAY")) return SKColors.LightGray;
                            if (text.Equals("LIGHTPINK")) return SKColors.LightPink;
                            if (text.Equals("LIMEGREEN")) return SKColors.LimeGreen;
                            break;
                        case 'M':
                            if (text.Equals("MINTCREAM")) return SKColors.MintCream;
                            if (text.Equals("MISTYROSE")) return SKColors.MistyRose;
                            break;
                        case 'O':
                            if (text.Equals("OLIVEDRAB")) return SKColors.OliveDrab;
                            if (text.Equals("ORANGERED")) return SKColors.OrangeRed;
                            break;
                        case 'P':
                            if (text.Equals("PALEGREEN")) return SKColors.PaleGreen;
                            if (text.Equals("PEACHPUFF")) return SKColors.PeachPuff;
                            break;
                        case 'R':
                            if (text.Equals("ROSYBROWN")) return SKColors.RosyBrown;
                            if (text.Equals("ROYALBLUE")) return SKColors.RoyalBlue;
                            break;
                        case 'S':
                            if (text.Equals("SLATEBLUE")) return SKColors.SlateBlue;
                            if (text.Equals("SLATEGRAY")) return SKColors.SlateGray;
                            if (text.Equals("STEELBLUE")) return SKColors.SteelBlue;
                            break;
                        case 'T': if (text.Equals("TURQUOISE")) return SKColors.Turquoise; break;
                    }
                    break;
                case 10:
                    switch (text[0])
                    {
                        case 'A': if (text.Equals("AQUAMARINE")) return SKColors.Aquamarine; break;
                        case 'B': if (text.Equals("BLUEVIOLET")) return SKColors.BlueViolet; break;
                        case 'C': if (text.Equals("CHARTREUSE")) return SKColors.Chartreuse; break;
                        case 'D':
                            if (text.Equals("DARKORANGE")) return SKColors.DarkOrange;
                            if (text.Equals("DARKORCHID")) return SKColors.DarkOrchid;
                            if (text.Equals("DARKSALMON")) return SKColors.DarkSalmon;
                            if (text.Equals("DARKVIOLET")) return SKColors.DarkViolet;
                            if (text.Equals("DODGERBLUE")) return SKColors.DodgerBlue;
                            break;
                        case 'G': if (text.Equals("GHOSTWHITE")) return SKColors.GhostWhite; break;
                        case 'L':
                            if (text.Equals("LIGHTCORAL")) return SKColors.LightCoral;
                            if (text.Equals("LIGHTGREEN")) return SKColors.LightGreen;
                            break;
                        case 'M': if (text.Equals("MEDIUMBLUE")) return SKColors.MediumBlue; break;
                        case 'P':
                            if (text.Equals("PAPAYAWHIP")) return SKColors.PapayaWhip;
                            if (text.Equals("POWDERBLUE")) return SKColors.PowderBlue;
                            break;
                        case 'S': if (text.Equals("SANDYBROWN")) return SKColors.SandyBrown; break;
                        case 'W': if (text.Equals("WHITESMOKE")) return SKColors.WhiteSmoke; break;
                    }
                    break;
                case 11:
                    switch (text[0])
                    {
                        case 'D':
                            if (text.Equals("DARKMAGENTA")) return SKColors.DarkMagenta;
                            if (text.Equals("DEEPSKYBLUE")) return SKColors.DeepSkyBlue;
                            break;
                        case 'F':
                            if (text.Equals("FLORALWHITE")) return SKColors.FloralWhite;
                            if (text.Equals("FORESTGREEN")) return SKColors.ForestGreen;
                            break;
                        case 'G': if (text.Equals("GREENYELLOW")) return SKColors.GreenYellow; break;
                        case 'L':
                            if (text.Equals("LIGHTSALMON")) return SKColors.LightSalmon;
                            if (text.Equals("LIGHTYELLOW")) return SKColors.LightYellow;
                            break;
                        case 'N': if (text.Equals("NAVAJOWHITE")) return SKColors.NavajoWhite; break;
                        case 'S':
                            if (text.Equals("SADDLEBROWN")) return SKColors.SaddleBrown;
                            if (text.Equals("SPRINGGREEN")) return SKColors.SpringGreen;
                            break;
                        case 'T':
                            text.Equals("TRANSPARENT");
                            return SKColors.Transparent;
                        case 'Y': if (text.Equals("YELLOWGREEN")) return SKColors.YellowGreen; break;
                    }
                    break;
                case 12:
                    switch (text[0])
                    {
                        case 'A': if (text.Equals("ANTIQUEWHITE")) return SKColors.AntiqueWhite; break;
                        case 'D': if (text.Equals("DARKSEAGREEN")) return SKColors.DarkSeaGreen; break;
                        case 'L':
                            if (text.Equals("LIGHTSKYBLUE")) return SKColors.LightSkyBlue;
                            if (text.Equals("LEMONCHIFFON")) return SKColors.LemonChiffon;
                            break;
                        case 'M':
                            if (text.Equals("MEDIUMORCHID")) return SKColors.MediumOrchid;
                            if (text.Equals("MEDIUMPURPLE")) return SKColors.MediumPurple;
                            if (text.Equals("MIDNIGHTBLUE")) return SKColors.MidnightBlue;
                            break;
                    }
                    break;
                case 13:
                    switch (text[0])
                    {
                        case 'D':
                            if (text.Equals("DARKSLATEBLUE")) return SKColors.DarkSlateBlue;
                            if (text.Equals("DARKSLATEGRAY")) return SKColors.DarkSlateGray;
                            if (text.Equals("DARKGOLDENROD")) return SKColors.DarkGoldenrod;
                            if (text.Equals("DARKTURQUOISE")) return SKColors.DarkTurquoise;
                            break;
                        case 'L':
                            if (text.Equals("LIGHTSEAGREEN")) return SKColors.LightSeaGreen;
                            if (text.Equals("LAVENDERBLUSH")) return SKColors.LavenderBlush;
                            break;
                        case 'P':
                            if (text.Equals("PALEGOLDENROD")) return SKColors.PaleGoldenrod;
                            if (text.Equals("PALETURQUOISE")) return SKColors.PaleTurquoise;
                            if (text.Equals("PALEVIOLETRED")) return SKColors.PaleVioletRed;
                            break;
                    }
                    break;
                case 14:
                    switch (text[0])
                    {
                        case 'B': if (text.Equals("BLANCHEDALMOND")) return SKColors.BlanchedAlmond; break;
                        case 'C': if (text.Equals("CORNFLOWERBLUE")) return SKColors.CornflowerBlue; break;
                        case 'D': if (text.Equals("DARKOLIVEGREEN")) return SKColors.DarkOliveGreen; break;
                        case 'L':
                            if (text.Equals("LIGHTSLATEGRAY")) return SKColors.LightSlateGray;
                            if (text.Equals("LIGHTSTEELBLUE")) return SKColors.LightSteelBlue;
                            break;
                        case 'M': if (text.Equals("MEDIUMSEAGREEN")) return SKColors.MediumSeaGreen; break;
                    }
                    break;
                case 15:
                    if (text.Equals("MEDIUMSLATEBLUE")) return SKColors.MediumSlateBlue;
                    if (text.Equals("MEDIUMTURQUOISE")) return SKColors.MediumTurquoise;
                    if (text.Equals("MEDIUMVIOLETRED")) return SKColors.MediumVioletRed;
                    break;
                case 16:
                    if (text.Equals("MEDIUMAQUAMARINE")) return SKColors.MediumAquamarine;
                    break;
                case 17:
                    if (text.Equals("MEDIUMSPRINGGREEN")) return SKColors.MediumSpringGreen;
                    break;
                case 20:
                    if (text.Equals("LIGHTGOLDENRODYELLOW")) return SKColors.LightGoldenrodYellow;
                    break;
            }
        }
        return SKColors.Transparent;
    }

    internal static SKColor ArgbStringToKnownColor(string argbString)
    {
        var key = argbString.Trim().ToUpper(CultureInfo.InvariantCulture);
        if (s_knownArgbColors.TryGetValue(key, out var value))
            return value;
        return SKColors.Transparent;
    }
}
