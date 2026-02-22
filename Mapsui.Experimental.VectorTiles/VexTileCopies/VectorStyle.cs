using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using VexTile.Common.Enums;
using VexTile.Common.Sources;
using VexTile.Renderer.Mvt.AliFlux;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VexTile.Renderer.Mvt.AliFlux.Enums;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public class VectorStyle : IVectorStyle
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public string Hash { get; }

    public List<Layer> Layers { get; } = new List<Layer>();

    public Dictionary<string, Source> Sources { get; } = new Dictionary<string, Source>();

    public Dictionary<string, object> Metadata { get; }

    public string CustomStyle { get; set; }

    public VectorStyle(VectorStyleKind style, double scale = 1.0, string customStyle = null)
    {
        CustomStyle = customStyle;
        string text;
        if (style == VectorStyleKind.Custom)
        {
            if (string.IsNullOrWhiteSpace(CustomStyle))
                throw new VectorStyleException("FATAL ERROR: Style could not be loaded!");
            text = CustomStyle;
        }
        else
        {
            text = VectorStyleReader.GetStyle(style);
        }

        var jObject = JObject.Parse(text);
        if (jObject["metadata"] != null)
            Metadata = jObject["metadata"].ToObject<Dictionary<string, object>>();

        var jToken = jObject["sources"];
        if (jToken != null)
        {
            foreach (var item in (IEnumerable<JToken>)jToken)
            {
                var source = new Source();
                if (item is JProperty prop && prop.Value is IDictionary<string, JToken> dictionary)
                {
                    source.Name = prop.Name;
                    if (dictionary.TryGetValue("url", out var value))
                        source.URL = SimplifyJson(value) as string;
                    if (dictionary.TryGetValue("type", out var value2))
                        source.Type = SimplifyJson(value2) as string;
                    if (dictionary.TryGetValue("minzoom", out var value3))
                        source.MinZoom = Convert.ToDouble(SimplifyJson(value3));
                    if (dictionary.TryGetValue("maxzoom", out var value4))
                        source.MaxZoom = Convert.ToDouble(SimplifyJson(value4));
                    Sources[prop.Name] = source;
                }
            }
        }

        var jToken2 = jObject["layers"];
        var num = 0;
        if (jToken2 is JArray source2)
        {
            foreach (var item2 in source2.OfType<JObject>())
            {
                var layer = new Layer { Index = num };
                if (((IDictionary<string, JToken>)item2).TryGetValue("minzoom", out var value5))
                    layer.MinZoom = Convert.ToDouble(SimplifyJson(value5));
                if (((IDictionary<string, JToken>)item2).TryGetValue("maxzoom", out var value6))
                    layer.MaxZoom = Convert.ToDouble(SimplifyJson(value6));
                if (((IDictionary<string, JToken>)item2).TryGetValue("id", out var value7))
                    layer.ID = SimplifyJson(value7) as string;
                if (((IDictionary<string, JToken>)item2).TryGetValue("type", out var value8))
                    layer.Type = SimplifyJson(value8) as string;
                if (((IDictionary<string, JToken>)item2).TryGetValue("source", out var value9))
                {
                    layer.SourceName = SimplifyJson(value9) as string;
                    layer.Source = Sources[layer.SourceName];
                }
                if (((IDictionary<string, JToken>)item2).TryGetValue("source-layer", out var value10))
                    layer.SourceLayer = SimplifyJson(value10) as string;
                if (((IDictionary<string, JToken>)item2).TryGetValue("paint", out var value11))
                    layer.Paint = SimplifyJson(value11) as Dictionary<string, object>;
                if (((IDictionary<string, JToken>)item2).TryGetValue("layout", out var value12))
                    layer.Layout = SimplifyJson(value12) as Dictionary<string, object>;
                if (((IDictionary<string, JToken>)item2).TryGetValue("filter", out var value13))
                {
                    var token = value13 as JArray;
                    layer.Filter = SimplifyJson(token) as object[];
                }
                Layers.Add(layer);
                num++;
            }
        }

        Hash = Utils.Sha256(text);
    }

    public void SetSourceProvider(string name, IBaseTileSource provider)
    {
        Sources[name].Provider = provider;
    }

    private static object SimplifyJson(JToken token)
    {
        if (token.Type == JTokenType.Object && token is IDictionary<string, JToken> source)
            return source.Select(pair => new KeyValuePair<string, object>(pair.Key, SimplifyJson(pair.Value)))
                .ToDictionary(key => key.Key, value => value.Value);
        if (token.Type == JTokenType.Array && token is JArray source2)
            return source2.Select(SimplifyJson).ToArray();
        return token.ToObject<object>();
    }

    public Brush[] GetStyleByType(string type, double zoom, double scale = 1.0)
    {
        var list = new List<Brush>();
        foreach (var layer in Layers)
        {
            if (layer.Type == type)
            {
                var attributes = new Dictionary<string, object>
                {
                    ["$type"] = "",
                    ["$id"] = "",
                    ["$zoom"] = zoom
                };
                list.Add(ParseStyle(layer, scale, attributes));
            }
        }
        return list.ToArray();
    }

    public Brush ParseStyle(Layer layer, double scale, Dictionary<string, object> attributes)
    {
        var paint = layer.Paint;
        var layout = layer.Layout;
        var index = layer.Index;
        var brush = new Brush { ZIndex = index, Layer = layer };
        var paint2 = brush.Paint = new Paint();

        if (paint != null)
        {
            if (paint.TryGetValue("fill-color", out var value))
                paint2.FillColor = ParseColor(GetValue(value, attributes));
            if (paint.TryGetValue("background-color", out var value2))
                paint2.BackgroundColor = ParseColor(GetValue(value2, attributes));
            if (paint.TryGetValue("text-color", out var value3))
                paint2.TextColor = ParseColor(GetValue(value3, attributes));
            if (paint.TryGetValue("line-color", out var value4))
                paint2.LineColor = ParseColor(GetValue(value4, attributes));
            if (paint.TryGetValue("line-pattern", out var value5))
                paint2.LinePattern = (string)GetValue(value5, attributes);
            if (paint.TryGetValue("background-pattern", out var value6))
                paint2.BackgroundPattern = (string)GetValue(value6, attributes);
            if (paint.TryGetValue("fill-pattern", out var value7))
                paint2.FillPattern = (string)GetValue(value7, attributes);
            if (paint.TryGetValue("text-opacity", out var value8))
                paint2.TextOpacity = Convert.ToDouble(GetValue(value8, attributes));
            if (paint.TryGetValue("icon-opacity", out var value9))
                paint2.IconOpacity = Convert.ToDouble(GetValue(value9, attributes));
            if (paint.TryGetValue("line-opacity", out var value10))
                paint2.LineOpacity = Convert.ToDouble(GetValue(value10, attributes));
            if (paint.TryGetValue("fill-opacity", out var value11))
                paint2.FillOpacity = Convert.ToDouble(GetValue(value11, attributes));
            if (paint.TryGetValue("background-opacity", out var value12))
                paint2.BackgroundOpacity = Convert.ToDouble(GetValue(value12, attributes));
            if (paint.TryGetValue("line-width", out var value13))
                paint2.LineWidth = Convert.ToDouble(GetValue(value13, attributes)) * scale;
            if (paint.TryGetValue("line-offset", out var value14))
                paint2.LineOffset = Convert.ToDouble(GetValue(value14, attributes)) * scale;
            if (paint.TryGetValue("line-dasharray", out var value15))
            {
                var source = GetValue(value15, attributes) as object[];
                paint2.LineDashArray = source.Select(item => Convert.ToDouble(item) * scale).ToArray();
            }
            if (paint.TryGetValue("text-halo-color", out var value16))
                paint2.TextStrokeColor = ParseColor(GetValue(value16, attributes));
            if (paint.TryGetValue("text-halo-width", out var value17))
                paint2.TextStrokeWidth = Convert.ToDouble(GetValue(value17, attributes)) * scale;
            if (paint.TryGetValue("text-halo-blur", out var value18))
                paint2.TextStrokeBlur = Convert.ToDouble(GetValue(value18, attributes)) * scale;
        }

        if (layout != null)
        {
            if (layout.TryGetValue("line-cap", out var value19))
            {
                switch ((string)GetValue(value19, attributes))
                {
                    case "butt": paint2.LineCap = PenLineCap.Flat; break;
                    case "round": paint2.LineCap = PenLineCap.Round; break;
                    case "square": paint2.LineCap = PenLineCap.Square; break;
                }
            }
            if (layout.TryGetValue("visibility", out var value20))
                paint2.Visibility = (string)GetValue(value20, attributes) == "visible";
            if (layout.TryGetValue("text-field", out var value21))
            {
                brush.TextField = (string)GetValue(value21, attributes);
                brush.Text = Regex.Replace(brush.TextField, "\\{([A-Za-z0-9\\-\\:_]+)\\}", delegate (Match m)
                {
                    var key = StripBraces(m.Value);
                    return attributes.TryGetValue(key, out var value30) ? value30.ToString() : string.Empty;
                }).Trim();
            }
            if (layout.TryGetValue("text-font", out var value22))
                paint2.TextFont = ((object[])GetValue(value22, attributes)).Select(item => (string)item).ToArray();
            if (layout.TryGetValue("text-size", out var value23))
                paint2.TextSize = Convert.ToDouble(GetValue(value23, attributes)) * scale;
            if (layout.TryGetValue("text-max-width", out var value24))
                paint2.TextMaxWidth = Convert.ToDouble(GetValue(value24, attributes)) * scale;
            if (layout.TryGetValue("text-offset", out var value25))
            {
                var array = (object[])GetValue(value25, attributes);
                paint2.TextOffset = new VexTile.Renderer.Mvt.AliFlux.Drawing.Point(
                    Convert.ToDouble(array[0]) * scale, Convert.ToDouble(array[1]) * scale);
            }
            if (layout.TryGetValue("text-optional", out var value26))
                paint2.TextOptional = (bool)GetValue(value26, attributes);
            if (layout.TryGetValue("text-transform", out var value27))
            {
                switch ((string)GetValue(value27, attributes))
                {
                    case "none": paint2.TextTransform = TextTransform.None; break;
                    case "uppercase": paint2.TextTransform = TextTransform.Uppercase; break;
                    case "lowercase": paint2.TextTransform = TextTransform.Lowercase; break;
                }
            }
            if (layout.TryGetValue("icon-size", out var value28))
                paint2.IconScale = Convert.ToDouble(GetValue(value28, attributes)) * scale;
            if (layout.TryGetValue("icon-image", out var value29))
                paint2.IconImage = (string)GetValue(value29, attributes);
        }

        return brush;
    }

    private static string StripBraces(string s)
    {
        var length = s.Length;
        Span<char> buffer = length <= 128 ? stackalloc char[length] : new char[length];
        var pos = 0;
        for (var i = 0; i < length; i++)
        {
            var c = s[i];
            if (c != '{' && c != '}')
                buffer[pos++] = c;
        }
        return new string(buffer[..pos]);
    }

    private static SKColor HslaToColor(double ta, double th, double ts, double tl)
    {
        var num = th / 365.0;
        var num2 = 0.0;
        var num3 = 0.0;
        var num4 = 0.0;
        var num5 = ts / 100.0;
        var num6 = tl / 100.0;
        if (!num6.BasicallyEqualTo(0.0))
        {
            if (!num5.BasicallyEqualTo(0.0))
            {
                var num7 = num6 < 0.5 ? num6 * (1.0 + num5) : num6 + num5 - num6 * num5;
                var temp = 2.0 * num6 - num7;
                num2 = GetColorComponent(temp, num7, num + 0.333333333333333);
                num3 = GetColorComponent(temp, num7, num);
                num4 = GetColorComponent(temp, num7, num - 0.333333333333333);
            }
            else
            {
                num2 = num3 = num4 = num6;
            }
        }
        var red = 255.0 * num2 > 255.0 ? byte.MaxValue : (byte)(255.0 * num2);
        var green = 255.0 * num3 > 255.0 ? byte.MaxValue : (byte)(255.0 * num3);
        var blue = 255.0 * num4 > 255.0 ? byte.MaxValue : (byte)(255.0 * num4);
        var alpha = (byte)ta;
        return SKColorFactory.MakeColor(red, green, blue, alpha, "HslaToColor");
    }

    private static double GetColorComponent(double temp1, double temp2, double temp3)
    {
        temp3 = MoveIntoRange(temp3);
        if (temp3 < 0.166666666666667)
            return temp1 + (temp2 - temp1) * 6.0 * temp3;
        if (temp3 < 0.5)
            return temp2;
        if (temp3 >= 0.666666666666667)
            return temp1;
        return temp1 + (temp2 - temp1) * (0.666666666666667 - temp3) * 6.0;
    }

    private static double MoveIntoRange(double temp3)
    {
        if (temp3 < 0.0)
            return temp3 + 1.0;
        if (temp3 <= 1.0)
            return temp3;
        return temp3 - 1.0;
    }

    private SKColor ParseColor(object iColor)
    {
        var provider = new CultureInfo("en-US", useUserOverride: true);
        if (iColor is Color color)
            return SKColorFactory.MakeColor(color.R, color.G, color.B, color.A, "ParseColor");
        if (iColor is SKColor color2)
            return SKColorFactory.LogColor(color2, "ParseColor");
        var text = (string)iColor;
        if (text[0] == '#')
            return SKColorFactory.LogColor(SKColor.Parse(text), "ParseColor");
        if (text.StartsWith("hsl("))
        {
            var array = text.Replace('%', '\0').Split(new char[] { ',', '(', ')' });
            var th = double.Parse(array[1], provider);
            var ts = double.Parse(array[2], provider);
            var tl = double.Parse(array[3], provider);
            return HslaToColor(255.0, th, ts, tl);
        }
        if (text.StartsWith("hsla("))
        {
            var array2 = text.Replace('%', '\0').Split(new char[] { ',', '(', ')' });
            return HslaToColor(
                ta: double.Parse(array2[4], provider) * 255.0,
                th: double.Parse(array2[1], provider),
                ts: double.Parse(array2[2], provider),
                tl: double.Parse(array2[3], provider));
        }
        if (text.StartsWith("rgba("))
        {
            var array3 = text.Replace('%', '\0').Split(new char[] { ',', '(', ')' });
            var num = double.Parse(array3[1], provider);
            var num2 = double.Parse(array3[2], provider);
            var num3 = double.Parse(array3[3], provider);
            var num4 = double.Parse(array3[4], provider) * 255.0;
            return SKColorFactory.MakeColor((byte)num, (byte)num2, (byte)num3, (byte)num4, "ParseColor");
        }
        if (text.StartsWith("rgb("))
        {
            var array4 = text.Replace('%', '\0').Split(new char[] { ',', '(', ')' });
            var num5 = double.Parse(array4[1], provider);
            var num6 = double.Parse(array4[2], provider);
            var num7 = double.Parse(array4[3], provider);
            return SKColorFactory.MakeColor((byte)num5, (byte)num6, (byte)num7, byte.MaxValue, "ParseColor");
        }
        try
        {
            return SKColorFactory.LogColor(ConvertFromString(text), "ParseColor");
        }
        catch (Exception value)
        {
            log.Error(value);
            throw new VectorStyleException("Not implemented color format: " + text);
        }
    }

    private static SKColor ConvertFromString(string value)
    {
        if (value == null)
            return SKColors.Transparent;
        return KnownColors.ParseColor(value);
    }

    public bool ValidateLayer(Layer layer, double zoom, Dictionary<string, object> attributes)
    {
        if (layer.MinZoom.HasValue && zoom < layer.MinZoom.Value)
            return false;
        if (layer.MaxZoom.HasValue && zoom > layer.MaxZoom.Value)
            return false;
        if (attributes != null && layer.Filter.Any() && !ValidateUsingFilter(layer.Filter, attributes))
            return false;
        return true;
    }

    protected Layer[] FindLayers(double zoom, string layerName, Dictionary<string, object> attributes)
    {
        var list = new List<Layer>();
        foreach (var layer in Layers)
        {
            if (layer.SourceLayer == layerName)
            {
                var flag = !layer.Filter.Any() || ValidateUsingFilter(layer.Filter, attributes);
                if (layer.MinZoom.HasValue && zoom < layer.MinZoom.Value)
                    flag = false;
                if (layer.MaxZoom.HasValue && zoom > layer.MaxZoom.Value)
                    flag = false;
                if (flag)
                    list.Add(layer);
            }
        }
        return list.ToArray();
    }

    private bool ValidateUsingFilter(object[] filterArray, Dictionary<string, object> attributes)
    {
        if (filterArray.Length == 0)
            Console.WriteLine("nothing");

        var text = filterArray[0] as string;
        switch (text)
        {
            case "all":
                foreach (object[] item in filterArray.Skip(1))
                {
                    if (!ValidateUsingFilter(item, attributes))
                        return false;
                }
                return true;
            case "any":
                foreach (object[] item2 in filterArray.Skip(1))
                {
                    if (ValidateUsingFilter(item2, attributes))
                        return true;
                }
                return false;
            case "none":
                {
                    var flag = false;
                    foreach (object[] item3 in filterArray.Skip(1))
                    {
                        if (ValidateUsingFilter(item3, attributes))
                            flag = true;
                    }
                    return !flag;
                }
            case "==":
            case "!=":
            case ">":
            case ">=":
            case "<":
            case "<=":
                {
                    var text2 = (string)filterArray[1];
                    if (text == "==")
                    {
                        if (!attributes.ContainsKey(text2))
                            return false;
                    }
                    else if (!attributes.ContainsKey(text2))
                        return true;

                    if (attributes[text2] is not IComparable)
                        throw new VectorStyleException("Comparing colors probably failed");

                    var comparable = (IComparable)attributes[text2];
                    var obj = GetValue(filterArray[2], attributes);
                    if (IsNumber(comparable) && IsNumber(obj))
                    {
                        comparable = Convert.ToDouble(comparable);
                        obj = Convert.ToDouble(obj);
                    }
                    if (comparable.GetType() != obj.GetType())
                        return false;

                    var num = comparable.CompareTo(obj);
                    switch (text)
                    {
                        case "==": return num == 0;
                        case "!=": return num != 0;
                        case ">": return num > 0;
                        case "<": return num < 0;
                        case ">=": return num >= 0;
                        case "<=": return num <= 0;
                    }
                    break;
                }
        }

        switch (text)
        {
            case "has":
                return attributes.ContainsKey((string)filterArray[1]);
            case "!has":
                return !attributes.ContainsKey((string)filterArray[1]);
            case "in":
                {
                    var key2 = (string)filterArray[1];
                    if (!attributes.TryGetValue(key2, out var value2))
                        return false;
                    foreach (var item4 in filterArray.Skip(2))
                    {
                        if (GetValue(item4, attributes).Equals(value2))
                            return true;
                    }
                    return false;
                }
            case "!in":
                {
                    var key = (string)filterArray[1];
                    if (!attributes.TryGetValue(key, out var value))
                        return true;
                    foreach (var item5 in filterArray.Skip(2))
                    {
                        if (GetValue(item5, attributes).Equals(value))
                            return false;
                    }
                    return true;
                }
            default:
                return false;
        }

        return false;
    }

    private object GetValue(object token, Dictionary<string, object> attributes = null)
    {
        if (token is string text && attributes != null)
        {
            if (text.Length == 0)
                return "";
            if (text[0] == '$')
                return GetValue(attributes[text]);
        }

        if (token.GetType().IsArray)
        {
            var src = (object[])token;
            var result = new object[src.Length];
            for (var i = 0; i < src.Length; i++)
                result[i] = GetValue(src[i], attributes);
            return result;
        }

        if (token is Dictionary<string, object> dictionary && dictionary.TryGetValue("stops", out var value) && value is object[] source)
        {
            var stopCount = source.Length;
            var stops = new (double Zoom, object Value)[stopCount];
            for (var i = 0; i < stopCount; i++)
            {
                var pair = (object[])source[i];
                stops[i] = (Convert.ToDouble(pair[0]), pair[1]);
            }
            var num = (double)attributes["$zoom"];
            var firstZoom = stops[0].Zoom;
            var lastZoom = stops[stopCount - 1].Zoom;
            var power = 1.0;
            var zoomA = firstZoom;
            var zoomB = lastZoom;
            var index = 0;
            var index2 = stopCount - 1;
            if (num <= firstZoom)
                return stops[0].Value;
            if (num >= lastZoom)
                return stops[stopCount - 1].Value;
            for (var i = 1; i < stopCount; i++)
            {
                var prevZoom = stops[i - 1].Zoom;
                var curZoom = stops[i].Zoom;
                if (num >= prevZoom && num <= curZoom)
                {
                    zoomA = prevZoom;
                    zoomB = curZoom;
                    index = i - 1;
                    index2 = i;
                    break;
                }
            }
            if (dictionary.TryGetValue("base", out var value2))
                power = Convert.ToDouble(GetValue(value2, attributes));
            return InterpolateValues(stops[index].Value, stops[index2].Value, zoomA, zoomB, num, power);
        }

        return token;
    }

    private static bool IsNumber(object value) =>
        value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;

    private object InterpolateValues(object startValue, object endValue, double zoomA, double zoomB, double zoom, double power, bool clamp = false)
    {
        if (startValue is string text)
        {
            var result2 = endValue as string;
            return Math.Abs(zoomA - zoom) <= Math.Abs(zoomB - zoom) ? text : result2;
        }
        if (startValue.GetType().IsArray)
        {
            var array = (object[])startValue;
            var array2 = (object[])endValue;
            var result = new object[array.Length];
            for (var i = 0; i < array.Length; i++)
                result[i] = InterpolateValues(array[i], array2[i], zoomA, zoomB, zoom, power, clamp);
            return result;
        }
        if (IsNumber(startValue))
        {
            var newMin = Convert.ToDouble(startValue);
            var newMax = Convert.ToDouble(endValue);
            return InterpolateRange(zoom, zoomA, zoomB, newMin, newMax, power, clamp);
        }
        throw new VectorStyleException("Unimplemented interpolation");
    }

    private static double InterpolateRange(double oldValue, double oldMin, double oldMax, double newMin, double newMax, double power, bool clamp = false)
    {
        var num = oldMax - oldMin;
        var num2 = oldValue - oldMin;
        var num3 = num == 0.0
            ? 0.0
            : power != 1.0
                ? (Math.Pow(power, num2) - 1.0) / (Math.Pow(power, num) - 1.0)
                : num2 / num;
        return num3 * (newMax - newMin) + newMin;
    }
}
