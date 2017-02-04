using Mapsui.Geometries;
using System.Collections.Generic;

namespace Mapsui.Rendering.Android.ExtensionMethods
{
    static class GeometryExtensions
    {
        public static float[] ToAndroid(this IList<Point> geometry)
        {
            var points = new float[geometry.Count * 2 * 2 - 2]; // Times two because x and y are in one array. Times two because of duplicate begin en end. Minus two because the very begin and end need no duplicate
            
            for (var i = 0; i < geometry.Count - 1; i++)
            {
                points[i * 4 + 0] = (float)geometry[i].X;
                points[i * 4 + 1] = (float)geometry[i].Y;
                points[i * 4 + 2] = (float)geometry[i + 1].X;
                points[i * 4 + 3] = (float)geometry[i + 1].Y;
            }
            return points;
        }
    }
}