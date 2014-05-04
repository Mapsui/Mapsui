using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Mapsui.Styles;
using AndroidColor = Android.Graphics.Color;
using AndroidPaint = Android.Graphics.Paint;
using Color = Mapsui.Styles.Color;

namespace Mapsui.Rendering.Android.ExtensionMethods
{
    static class StyleExtensions
    {
        public static AndroidColor ToAndroid(this Color color)
        {
            return new AndroidColor(color.R, color.G, color.B, color.A);
        }

        private static Paint ToAndroid(this Pen pen)
        {
            var paint = new Paint
            {
                AntiAlias = true,
                Color = pen.Color.ToAndroid(),
                StrokeWidth = (float)pen.Width,
                StrokeJoin = Paint.Join.Round
            };
            paint.SetStyle(Paint.Style.Stroke);
            return paint;
        }

        private static Paint ToAndroid(this Brush brush)
        {
            var paint = new Paint { AntiAlias = true, Color = brush.Color.ToAndroid() };
            paint.SetStyle(Paint.Style.Fill);
            return paint;
        }

        public static IEnumerable<Paint> ToAndroid(this IStyle style)
        {
            var vectorStyle = style as VectorStyle;
            if (vectorStyle == null) yield break;

            if (vectorStyle.Outline != null && vectorStyle.Outline.Color != null)
            {
                yield return vectorStyle.Outline.ToAndroid();
            }
            if (vectorStyle.Line != null && vectorStyle.Line.Color != null)
            {
                yield return vectorStyle.Line.ToAndroid();
            }
            if (vectorStyle.Fill != null && vectorStyle.Fill.Color != null)
            {
                yield return vectorStyle.Fill.ToAndroid();
            }
        }
    }
}