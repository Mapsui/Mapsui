using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering.iOS.ExtensionMethods;
using Mapsui.Styles;
using CoreAnimation;
using CoreGraphics;
using CoreGraphics;
using UIKit;

namespace Mapsui.Rendering.iOS
{
	public static class LineStringRenderer
	{
		public static void Draw(CALayer target, IViewport viewport, IStyle style, IFeature feature)
		{
			var lineString = ((LineString) feature.Geometry).Vertices;
			var path = ((LineString) feature.Geometry).Vertices.ToUIKit(viewport);
			var vectorStyle = (style as VectorStyle) ?? new VectorStyle();

			var shape = new CAShapeLayer
			{
				StrokeColor = vectorStyle.Line.Color.ToCG(),
				LineWidth = (float)vectorStyle.Line.Width,
				Path = path.CGPath
			};
			target.AddSublayer (shape);
		}
	}
}