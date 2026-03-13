using Mapsui.Widgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

public static class SkiaTextLayoutHelper
{
    public class Line
    {
        public string? Value { get; set; }
        public float Width { get; set; }
        public float Baseline { get; set; }
        public float BoundsLeft { get; set; }
        public float BoundsWidth { get; set; }
    }

    public static Line[] SplitLines(string? text, SKFont font, SKPaint paint, float maxWidth, string splitCharacter)
    {
        if (text == null)
            return [];

        var spaceWidth = font.MeasureText(" ", paint);
        var lines = text.Split('\n');

        return lines.SelectMany(line =>
        {
            var result = new List<Line>();
            string[] words;

            if (splitCharacter == string.Empty)
            {
                words = line.ToCharArray().Select(x => x.ToString()).ToArray();
                spaceWidth = 0;
            }
            else
            {
                words = line.Split(new[] { splitCharacter }, StringSplitOptions.None);
            }

            var lineResult = new StringBuilder();
            float width = 0;
            foreach (var word in words)
            {
                var wordWidth = font.MeasureText(word, paint);
                var wordWithSpaceWidth = wordWidth + spaceWidth;
                var wordWithSpace = word + splitCharacter;

                if (width + wordWidth > maxWidth)
                {
                    result.Add(new Line { Value = lineResult.ToString(), Width = width });
                    lineResult = new StringBuilder(wordWithSpace);
                    width = wordWithSpaceWidth;
                }
                else
                {
                    lineResult.Append(wordWithSpace);
                    width += wordWithSpaceWidth;
                }
            }

            result.Add(new Line { Value = lineResult.ToString(), Width = width });

            return result.ToArray();
        }).ToArray();
    }

    /// <summary>
    /// Layout text with word wrap and alignment, measuring the result size.
    /// Returns the lines and total size of the text block.
    /// </summary>
    public static (Line[] Lines, float Width, float Height) LayoutText(
        string? text, SKFont font, SKPaint paint, float maxWidth, Alignment alignment)
    {
        if (string.IsNullOrEmpty(text))
            return ([], 0, 0);

        font.MeasureText(text, out var rect, paint);
        var baseline = -rect.Top;
        var lineSpacing = font.Spacing;

        Line[] lines;
        if (maxWidth > 0 && (rect.Right - rect.Left) > maxWidth)
            lines = SplitLines(text, font, paint, maxWidth, " ");
        else
            lines = [new Line { Value = text, Width = font.MeasureText(text, paint) }];

        var width = 0f;
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i].Baseline = baseline + lineSpacing * i;
            if (lines[i].Width > width)
                width = lines[i].Width;
        }

        // Use font.Spacing (= ascent + descent + leading) as the height per line, matching
        // RichTextKit's (the previous implementation) MeasuredHeight behavior which also includes leading. 
        // Without leading, the title block would be shorter than RTK's, causing the subtitle to sit too close.
        var height = lineSpacing * lines.Length;
        return (lines, width, height);
    }

    /// <summary>
    /// Draw text lines onto a canvas with the given alignment.
    /// </summary>
    public static void DrawTextBlock(SKCanvas canvas, Line[] lines, SKFont font, SKPaint paint,
        float x, float y, float blockWidth, Alignment alignment)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.Value))
                continue;

            var lineX = alignment switch
            {
                Alignment.Center => x + (blockWidth - line.Width) * 0.5f,
                Alignment.Right => x + blockWidth - line.Width,
                _ => x,
            };

            canvas.DrawText(line.Value, lineX, y + line.Baseline, SKTextAlign.Left, font, paint);
        }
    }
}
