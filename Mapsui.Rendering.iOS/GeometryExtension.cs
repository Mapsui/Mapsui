using Mapsui.Geometries;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.iOS
{
	static class GeometryExtension
	{
		public static System.Drawing.Point OffSet;

		public static PointF ToUIKit(this Point point)
		{
		    double xo = OffSet.X;
		    double yo = OffSet.Y;
		    return new PointF((float)(point.X - (xo)), (float)(point.Y - yo));
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

			pathFigure.MoveTo(ToUIKit(startPos));

			for(int i = 1; i < linearRing.Vertices.Count; i++)
			{
				var pos = linearRing.Vertices[i];
				var screenPos = viewport.WorldToScreen(pos);
				pathFigure.AddLineTo(ToUIKit(screenPos));
			}
			pathFigure.ClosePath();
			
			return pathFigure;
		}
	}
}