﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class ClippingExtension
    {
        /// <summary>
        /// Converts a LineString (list of Mapsui points) in world coordinates to a Skia path
        /// </summary>
        /// <param name="lineString">List of points in Mapsui world coordinates</param>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
        /// <returns></returns>
        public static SKPath ToSkiaPath(this IEnumerable<Point> lineString, IReadOnlyViewport viewport, SKRect clipRect)
        {
            // First convert List<Points> to screen coordinates
            var vertices = WorldToScreen(viewport, lineString);

            var path = new SKPath();
            SKPoint lastPoint = SKPoint.Empty;

            for (var i = 1; i < vertices.Count; i++)
            {
                // Check each part of LineString, if it is inside or intersects the clipping rectangle
                var intersect = LiangBarskyClip(vertices[i - 1], vertices[i], clipRect, out var intersectionPoint1, out var intersectionPoint2);

                if (intersect != Intersection.CompleteOutside)
                {
                    // If the last point isn't the same as actuall starting point ...
                    if (lastPoint.IsEmpty || !lastPoint.Equals(intersectionPoint1))
                    {
                        // ... than move to this point
                        path.MoveTo(intersectionPoint1);
                    }
                    // Draw line
                    path.LineTo(intersectionPoint2);

                    // Save last end point for later use
                    lastPoint = intersectionPoint2;
                }
            }
            return path;
        }

        /// <summary>
        /// Converts a Polygon into a SKPath, that is clipped to cliptRect, where exterior is bigger than interior
        /// </summary>
        /// <param name="polygon">Polygon to convert</param>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
        /// <param name="strokeWidth">StrokeWidth for inflating cliptRect</param>
        /// <returns></returns>
        public static SKPath ToSkiaPath(this Polygon polygon, IReadOnlyViewport viewport, SKRect clipRect, float strokeWidth)
        {
            // Reduce exterior ring to parts, that are visible in clipping rectangle
            // Inflate clipRect, so that we could be sure, nothing of stroke is visible on screen
            var exterior = ReducePointsToClipRect(polygon.ExteriorRing.Vertices, viewport, SKRect.Inflate(clipRect, strokeWidth * 2, strokeWidth * 2));

            // Create path for exterior and interior parts
            var path = new SKPath();

            if (exterior.Count == 0)
                return path;

            // Draw exterior path
            path.MoveTo(exterior[0]);

            for (var i = 1; i < exterior.Count; i++)
            {
                path.LineTo(exterior[i]);
            }

            // Close exterior path
            path.Close();

            foreach (var interiorRing in polygon.InteriorRings)
            {
                // note: For Skia inner rings need to be clockwise and outer rings
                // need to be counter clockwise (if this is the other way around it also
                // seems to work)
                // this is not a requirement of the OGC polygon.

                // Reduce interior ring to parts, that are visible in clipping rectangle
                var interior = ReducePointsToClipRect(interiorRing.Vertices, viewport, SKRect.Inflate(clipRect, strokeWidth, strokeWidth));

                if (interior.Count == 0)
                    continue;

                // Draw interior pathes
                path.MoveTo(interior[0]);

                for (var i = 1; i < interior.Count; i++)
                {
                    path.LineTo(interior[i]);
                }
            }

            // Close interior pathes
            path.Close();

            return path;
        }

        /// <summary>
        /// Comparer for each side of the clipping rectangle to check, if a point 
        /// is inside or outside of this edge.
        /// There are 4 edges (left, top, right, bottom).
        /// </summary>
        private static readonly Func<SKPoint, SKRect, bool>[] Comparer = new Func<SKPoint, SKRect, bool>[]
        {
            (point, rect) => point.X > rect.Left, // Left edge of rect
            (point, rect) => point.Y > rect.Top, // Top edge of rect
            (point, rect) => point.X < rect.Right, // Right edge of rect
            (point, rect) => point.Y < rect.Bottom, // Bottom edge of rect
        };

        /// <summary>
        /// Calculates the intersection point of line between pointStart and pointEnd 
        /// and the edge.
        /// There are 4 edges (left, top, right, bottom).
        /// </summary>
        private static readonly Func<SKPoint, SKPoint, SKRect, SKPoint>[] Intersecter = new Func<SKPoint, SKPoint, SKRect, SKPoint>[]
        {
            (pointStart, pointEnd, rect) => new SKPoint(rect.Left, pointStart.Y + (rect.Left-pointStart.X)/(pointEnd.X-pointStart.X)*(pointEnd.Y-pointStart.Y)), // Left edge of rect
            (pointStart, pointEnd, rect) => new SKPoint(pointStart.X + (rect.Top-pointStart.Y)/(pointEnd.Y-pointStart.Y)*(pointEnd.X-pointStart.X), rect.Top),   // Top edge of rect
            (pointStart, pointEnd, rect) => new SKPoint(rect.Right, pointEnd.Y + (rect.Right-pointEnd.X)/(pointStart.X-pointEnd.X)*(pointStart.Y-pointEnd.Y)),   // Right edge of rect
            (pointStart, pointEnd, rect) => new SKPoint(pointEnd.X + (rect.Bottom-pointEnd.Y)/(pointStart.Y-pointEnd.Y)*(pointStart.X-pointEnd.X), rect.Bottom), // Bottom edge of rect
        };

        /// <summary>
        /// Reduce list of points, so that all are inside of cliptRect
        /// See https://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman_algorithm
        /// </summary>
        /// <param name="points">List of points to reduce</param>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="clipRect">Rectangle to clip to. All points outside aren't drawn.</param>
        /// <returns></returns>
        private static List<SKPoint> ReducePointsToClipRect(IEnumerable<Point> points, IReadOnlyViewport viewport, SKRect clipRect)
        {
            var output = WorldToScreen(viewport, points);

            // Do this for the 4 edges (left, top, right, bottom) of clipping rectangle
            for (var j = 0; j < 4; j++)
            {
                // If there aren't any points to reduce
                if (output == null || output.Count == 0)
                    return new List<SKPoint>();

                // New input list is the last output list of points
                var input = new List<SKPoint>(output);

                output.Clear();

                var pointStart = input.Last();

                foreach (var pointEnd in input)
                {
                    // Is pointEnd inside of clipping rectangle regarding this edge
                    if (Comparer[j](pointEnd, clipRect))
                    {
                        // Is pointStart outside of clipping rectangle regarding this edge
                        if (!Comparer[j](pointStart, clipRect))
                        {
                            // Yes, than line is coming from outside to inside, so calculate intersection
                            output.Add(Intersecter[j](pointStart, pointEnd, clipRect));
                        }
                        // pointEnd is inside, so add it to points list
                        output.Add(pointEnd);
                    }
                    // Is pointStart inside of clipping rectangle regarding this edge
                    else if (Comparer[j](pointStart, clipRect))
                    {
                        // Yes, than line is coming from inside to outside, so calculate intersection
                        output.Add(Intersecter[j](pointStart, pointEnd, clipRect));
                    }

                    // Set next pointStart
                    pointStart = pointEnd;
                }
            }

            return output;
        }

        /// <summary>
        /// Convert a list of Mapsui points in world coordinates to SKPoint in screen coordinates
        /// </summary>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="points">List of points in Mapsui world coordinates</param>
        /// <returns>List of screen coordinates in SKPoint</returns>
        private static List<SKPoint> WorldToScreen(IReadOnlyViewport viewport, IEnumerable<Point> points)
        {
            var result = new List<SKPoint>();
            var screenCenterX = viewport.Width * 0.5;
            var screenCenterY = viewport.Height * 0.5;
            var centerX = viewport.Center.X;
            var centerY = viewport.Center.Y;
            var resolution = 1.0 / viewport.Resolution;
            var rotation = viewport.Rotation / 180f * Math.PI;
            var sin = Math.Sin(rotation);
            var cos = Math.Cos(rotation);

            foreach (var point in points)
            {
                var screenX = (point.X - centerX) * resolution;
                var screenY = (centerY - point.Y) * resolution;

                if (viewport.IsRotated)
                {
                    var newX = screenX * cos - screenY * sin;
                    var newY = screenX * sin + screenY * cos;
                    screenX = newX;
                    screenY = newY;
                }

                screenX += screenCenterX;
                screenY += screenCenterY;

                result.Add(new SKPoint((float)screenX, (float)screenY));
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
            var p = new [] { -vx, vx, -vy, vy };
            var q = new [] { point1.X - clipRect.Left, clipRect.Right - point1.X, point1.Y - clipRect.Top, clipRect.Bottom - point1.Y };
            var u1 = float.NegativeInfinity;
            var u2 = float.PositiveInfinity;

            // Up to now both points are inside the clipping rectangle
            intersectionPoint1 = point1;
            intersectionPoint2 = point2;

            // Check, if points are complete outside
            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0)
                {
                    // Line is parallel to one side
                    if (q[i] < 0)
                        return Intersection.CompleteOutside;
                }
                else
                {
                    // Calculate intersection points
                    var t = q[i] / p[i];
                    if (p[i] < 0 && u1 < t)
                        u1 = t;
                    else if (p[i] > 0 && u2 > t)
                        u2 = t;
                }
            }

            // Are both points outside and don't intersect?
            if (u1 > u2)
                return Intersection.CompleteOutside;

            // Are both points inside and don't intersect?
            if (u1 < 0 && u2 > 1)
            {
                return Intersection.CompleteInside;
            }

            // Are both points outside, but intersect on both sides?
            if (u1 > 0 && u2 < 1)
            {
                intersectionPoint1.X = point1.X + u1 * vx;
                intersectionPoint1.Y = point1.Y + u1 * vy;
                intersectionPoint2.X = point1.X + u2 * vx;
                intersectionPoint2.Y = point1.Y + u2 * vy;

                return Intersection.Both;
            }

            // Is the first point outside and the second point inside?
            if (u1 > 0 && u1 < 1)
            {
                intersectionPoint1.X = point1.X + u1 * vx;
                intersectionPoint1.Y = point1.Y + u1 * vy;

                return Intersection.SecondInside;
            }

            // Is the first point inside and the second point outside?
            if (u2 > 0 && u2 < 1)
            {
                intersectionPoint2.X = point1.X + u2 * vx;
                intersectionPoint2.Y = point1.Y + u2 * vy;

                return Intersection.FirstInside;
            }

            return Intersection.Unknown;
        }
    }

    /// <summary>
    /// Type of intersection
    /// </summary>
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
