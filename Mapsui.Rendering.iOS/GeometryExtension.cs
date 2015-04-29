using Mapsui.Geometries;
using UIKit;
using System.Collections.Generic;
using CoreGraphics;
using CGPoint = Mapsui.Geometries.CGPoint;
using System.Linq;

namespace Mapsui.Rendering.iOS
{
	static class GeometryExtension
	{
		public static CoreGraphics.CGPoint OffSet;

		public static CGPoint ToUIKit(this CGPoint point)
		{
			double xo = OffSet.X;
			double yo = OffSet.Y;
			return new CGPoint((float)(point.X - (xo)), (float)(point.Y - yo));
		}

		public static UIBezierPath ToUIKit(this IEnumerable<CGPoint> points, IViewport viewport)
		{
			var pathGeometry = new UIBezierPath ();
		    points = points.ToList();
			if (points.Any()) {

				var first = points.FirstOrDefault ();
				var start = viewport.WorldToScreen (first);

				pathGeometry.MoveTo ((CGPoint)ToUIKit (start));

				for (int i = 1; i < points.Count (); i++) {
					var point = points.ElementAt (i);
					var p = viewport.WorldToScreen (point);
					pathGeometry.AddLineTo ((CGPoint)new CGPoint ((float)p.X, (float)p.Y));
				}
			}
			return pathGeometry;
		}

		public static UIBezierPath ToUIKit(this LineString lineString, IViewport viewport)
		{
			var pathGeometry = new UIBezierPath();
			pathGeometry.AppendPath(CreatePathFigure(lineString, viewport));
			return pathGeometry;
		}

		public static UIBezierPath ToUIKit(this MultiLineString multiLineString, IViewport viewport)
		{
			var group = new UIBezierPath();
			foreach (LineString lineString in multiLineString)
				group.AppendPath(ToUIKit(lineString, viewport));
			return group;
		}

		public static UIBezierPath ToUIKit(this LinearRing linearRing, IViewport viewport)
		{
			var pathGeometry = new UIBezierPath();
			pathGeometry.AppendPath(CreatePathFigure(linearRing, viewport));
			return pathGeometry;
		}

		public static UIBezierPath ToUIKit(this IEnumerable<LinearRing> linearRings, IViewport viewport)
		{
			var pathGeometry = new UIBezierPath();
			foreach (var linearRing in linearRings)
				pathGeometry.AppendPath(CreatePathFigure(linearRing, viewport));
			return pathGeometry;
		}

		public static UIBezierPath ToUIKit(this Polygon polygon,IViewport viewport)
		{
			var group = new UIBezierPath();
			group.UsesEvenOddFillRule = true;
			group.AppendPath(ToUIKit(polygon.ExteriorRing, viewport));
			group.AppendPath(ToUIKit(polygon.InteriorRings, viewport));
			return group;
		}

		public static UIBezierPath ToUIKit(this MultiPolygon geometry, IViewport viewport)
		{
			var group = new UIBezierPath();
			foreach(var polygon in geometry.Polygons)
				group.AppendPath(ToUIKit(polygon, viewport));

			return group;
		}

		private static UIBezierPath CreatePathFigure(LineString linearRing, IViewport viewport)
		{
			var pathFigure = new UIBezierPath();
			var start = linearRing.Vertices[0];
			var startPos = viewport.WorldToScreen(start);

			pathFigure.MoveTo((CGPoint)ToUIKit(startPos));

			for(int i = 1; i < linearRing.Vertices.Count; i++)
			{
				var pos = linearRing.Vertices[i];
				var screenPos = viewport.WorldToScreen(pos);
				pathFigure.AddLineTo((CGPoint)ToUIKit(screenPos));
			}
			pathFigure.ClosePath();

			return pathFigure;
		}
	}
}