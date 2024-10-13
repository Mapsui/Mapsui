using System;
using System.Collections.Generic;
using System.IO;
using Svg;
using Svg.Model;

namespace Mapsui.Styles;

public static class SvgColorModifier
{
    public static MemoryStream GetModifiedSvg(byte[] bytes, Color? fillColor = null, Color? strokeColor = null)
    {
        var svgFillColor = (System.Drawing.Color?)fillColor;
        var svgStrokeColor = (System.Drawing.Color?)strokeColor;

        using var memoryStream = new MemoryStream(bytes);
        var svgDocument = SvgExtensions.Open(memoryStream) ?? throw new Exception("Could not open stream as svg");

        var elements = GetAllElements(svgDocument.Children);
        foreach (var element in elements)
        {
            if (element.Fill is not null && svgFillColor is not null)
                element.Fill = new SvgColourServer(svgFillColor.Value);
            if (element.Stroke is not null && svgStrokeColor is not null)
                element.Stroke = new SvgColourServer(svgStrokeColor.Value);
        }

        var outputStream = new MemoryStream();
        svgDocument.Write(outputStream);
        return outputStream;
    }

    public static List<SvgElement> GetAllElements(SvgElementCollection elements)
    {
        var result = new List<SvgElement>();
        foreach (var element in elements)
        {
            result.Add(element);

            if (element.Children.Count > 0)
                result.AddRange(GetAllElements(element.Children));
        }
        return result;
    }
}
