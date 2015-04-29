using System.Collections.Generic;
using Mapsui.Styles;
using CoreGraphics;
using UIKit;

namespace Mapsui.Rendering.iOS.ExtensionMethods
{
	static class StyleExtensions
	{
		public static UIColor ToUIKit(this Color color)
		{
            return new UIColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}

        public static CGColor ToCG(this Color color)
        {
            return new CGColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

		private static UIColor ToUIKit(this Brush brush)
		{
			return brush.Color.ToUIKit();
		}

		public static IEnumerable<UIColor> ToUIKit(this IStyle style)
		{
			var vectorStyle = style as VectorStyle;
			if (vectorStyle == null) yield break;

			if (vectorStyle.Outline != null && vectorStyle.Outline.Color != null)
			{
				yield return vectorStyle.Outline.Color.ToUIKit();
			}
			if (vectorStyle.Line != null && vectorStyle.Line.Color != null)
			{
                yield return vectorStyle.Line.Color.ToUIKit();
			}
			if (vectorStyle.Fill != null && vectorStyle.Fill.Color != null)
			{
				yield return vectorStyle.Fill.ToUIKit();
			}
		}
	}
}