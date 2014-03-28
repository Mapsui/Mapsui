using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using System.Drawing;

namespace Mapsui.Rendering.iOS
{
	static class GeometryExtensions
	{
		public static PointF[] ToiOS(this IList<Mapsui.Geometries.Point> geometry)
		{
			var points = new PointF[geometry.Count]; // Times two because x and y are in one array. Times two because of duplicate begin en end. Minus two because the very begin and end need no duplicate

			for (var i = 0; i < geometry.Count; i++)
			{
				points [i] = new PointF ((float)geometry[i].X, (float)geometry[i].Y);

				//				points[i * 4 + 0] = (float)geometry[i].X;
				//				points[i * 4 + 1] = (float)geometry[i].Y;
				//				points[i * 4 + 2] = (float)geometry[i + 1].X;
				//				points[i * 4 + 3] = (float)geometry[i + 1].Y;
			}
			return points;
		}
	}
}