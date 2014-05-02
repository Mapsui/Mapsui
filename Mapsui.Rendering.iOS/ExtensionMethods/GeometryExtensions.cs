using System.Collections.Generic;
using System.Drawing;

namespace Mapsui.Rendering.iOS.ExtensionMethods
{
	static class GeometryExtensions
	{
		public static PointF[] ToiOS(this IList<Geometries.Point> geometry)
		{
			var points = new PointF[geometry.Count]; // Times two because x and y are in one array. Times two because of duplicate begin en end. Minus two because the very begin and end need no duplicate

			for (var i = 0; i < geometry.Count; i++)
			{
				points [i] = new PointF ((float)geometry[i].X, (float)geometry[i].Y);
			}
			return points;
		}
	}
}