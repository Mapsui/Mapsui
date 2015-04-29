using System.Collections.Generic;
using CoreGraphics;

namespace Mapsui.Rendering.iOS.ExtensionMethods
{
	static class GeometryExtensions
	{
		public static CGPoint[] ToiOS(this IList<Geometries.CGPoint> geometry)
		{
			var points = new CGPoint[geometry.Count]; // Times two because x and y are in one array. Times two because of duplicate begin en end. Minus two because the very begin and end need no duplicate

			for (var i = 0; i < geometry.Count; i++)
			{
				points [i] = new CGPoint ((float)geometry[i].X, (float)geometry[i].Y);
			}
			return points;
		}
	}
}