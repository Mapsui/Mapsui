using System.Collections.Generic;
using Mapsui.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class ClippingExtension
    {
        /// <summary>
        /// Converts a list of Mapsui points in screen coordinates to a Skia path
        /// </summary>
        /// <param name="lineString">List of points in Mapsui world coordinates</param>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
        /// <returns></returns>
        public static SKPath ToSkiaPath(this IEnumerable<Point> lineString, IViewport viewport, SKRect clipRect)
        {
            // First convert List<Points> to screen coordinates
            var vertices = WorldToScreen(viewport, lineString);

            var points = new SKPath();
            SKPoint lastPoint;

            for (var i = 1; i < vertices.Count; i++)
            {
                var intersect = LiangBarskyClip(vertices[i - 1], vertices[i], clipRect, out var intersectionPoint1, out var intersectionPoint2);

                if (intersect != Intersection.CompleteOutside)
                {
                    if (lastPoint.IsEmpty || !lastPoint.Equals(intersectionPoint1))
                    {
                        points.MoveTo(intersectionPoint1);
                    }
                    points.LineTo(intersectionPoint2);

                    lastPoint = intersectionPoint2;
                }
            }
            return points;
        }

        /// <summary>
        /// Convert a list of Mapsui points in world coordinates to SKPoint in screen coordinates
        /// </summary>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="points">List of points in Mapsui world coordinates</param>
        /// <returns>List of screen coordinates in SKPoint</returns>
        private static List<SKPoint> WorldToScreen(IViewport viewport, IEnumerable<Point> points)
        {
            var result = new List<SKPoint>();
            foreach (var point in points)
            {
                var screenPoint = viewport.WorldToScreen(point.X, point.Y);
                result.Add(new SKPoint((float)screenPoint.X, (float)screenPoint.Y));
            }
            return result;
        }

        /// <summary>
        /// A Liang-Barsky implementation to detect the intersection between a line and a rect.
        /// With this, all lines, that aren't visible on screen could be sorted out.
        /// Found at https://gist.github.com/ChickenProp/3194723
        /// </summary>
        /// <param name="point1">First point of line</param>
        /// <param name="point2">Second point of line</param>
        /// <param name="clipRect"></param>
        /// <param name="intersectionPoint1">First intersection point </param>
        /// <param name="intersectionPoint2">Second intersection point</param>
        /// <returns></returns>
        private static Intersection LiangBarskyClip(SKPoint point1, SKPoint point2, SKRect clipRect, out SKPoint intersectionPoint1, out SKPoint intersectionPoint2)
        {
            var vx = point2.X - point1.X;
            var vy = point2.Y - point1.Y;
            var p = new float[] { -vx, vx, -vy, vy };
            var q = new float[] { point1.X - clipRect.Left, clipRect.Right - point1.X, point1.Y - clipRect.Top, clipRect.Bottom - point1.Y };
            var u1 = float.NegativeInfinity;
            var u2 = float.PositiveInfinity;

            intersectionPoint1 = point1;
            intersectionPoint2 = point2;

            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0)
                {
                    if (q[i] < 0)
                        return Intersection.CompleteOutside;
                }
                else
                {
                    var t = q[i] / p[i];
                    if (p[i] < 0 && u1 < t)
                        u1 = t;
                    else if (p[i] > 0 && u2 > t)
                        u2 = t;
                }
            }

            if (u1 > u2)
                return Intersection.CompleteOutside;

            if (u1 < 0 && u2 > 1)
            {
                return Intersection.CompleteInside;
            }

            if (u1 > 0 && u2 < 1)
            {
                intersectionPoint1.X = point1.X + u1 * vx;
                intersectionPoint1.Y = point1.Y + u1 * vy;
                intersectionPoint2.X = point1.X + u2 * vx;
                intersectionPoint2.Y = point1.Y + u2 * vy;

                return Intersection.Both;
            }

            if (u1 > 0 && u1 < 1)
            {
                intersectionPoint1.X = point1.X + u1 * vx;
                intersectionPoint1.Y = point1.Y + u1 * vy;

                return Intersection.SecondInside;
            }

            if (u2 > 0 && u2 < 1)
            {
                intersectionPoint2.X = point1.X + u2 * vx;
                intersectionPoint2.Y = point1.Y + u2 * vy;

                return Intersection.FirstInside;
            }

            return Intersection.Unknown;
        }
    }

    public enum Intersection
    {
        CompleteInside,
        CompleteOutside,
        Both,
        FirstInside,
        SecondInside,
        Unknown
    }
}
