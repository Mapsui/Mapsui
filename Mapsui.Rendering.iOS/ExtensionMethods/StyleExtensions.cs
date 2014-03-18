using System;
using MonoTouch.UIKit;
using Mapsui.Styles;
using System.Collections.Generic;

namespace Mapsui.Rendering.iOS
{
	static class StyleExtensions
	{
		public static UIColor ToiOS(this Color color)
		{
			return new UIColor(color.R, color.G, color.B, color.A);
		}

		private static UIColor ToiOS(this Pen pen)
		{
			//			var paint = new UIColor
			//			{
			//				AntiAlias = true,
			//				Color = pen.Color.ToAndroid(),
			//				StrokeWidth = (float)pen.Width,
			//				StrokeJoin = Paint.Join.Round
			//			};
			//			paint.SetStyle(Paint.Style.Stroke);

			return pen.Color.ToiOS ();
		}

		private static UIColor ToiOS(this Brush brush)
		{
			//			var paint = new Paint { AntiAlias = true, Color = brush.Color.ToAndroid() };
			//			paint.SetStyle(Paint.Style.Fill);
			return brush.Color.ToiOS();
		}

		public static IEnumerable<UIColor> ToiOS(this IStyle style)
		{
			var vectorStyle = style as VectorStyle;
			if (vectorStyle == null) yield break;

			if (vectorStyle.Outline != null && vectorStyle.Outline.Color != null)
			{
				yield return vectorStyle.Outline.ToiOS();
			}
			if (vectorStyle.Line != null && vectorStyle.Line.Color != null)
			{
				yield return vectorStyle.Line.ToiOS();
			}
			if (vectorStyle.Fill != null && vectorStyle.Fill.Color != null)
			{
				yield return vectorStyle.Fill.ToiOS();
			}
		}
	}
}