using System;
using System.Collections.Generic;
using NLog;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VexTile.Renderer.Mvt.AliFlux.Enums;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public static class LineClipper
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    private static OutCode ComputeOutCode(double x, double y, Rect r)
    {
        OutCode outCode = OutCode.Inside;
        if (x < r.Left)
        {
            outCode |= OutCode.Left;
        }

        if (x > r.Right)
        {
            outCode |= OutCode.Right;
        }

        if (y < r.Top)
        {
            outCode |= OutCode.Top;
        }

        if (y > r.Bottom)
        {
            outCode |= OutCode.Bottom;
        }

        return outCode;
    }

    private static OutCode ComputeOutCode(Point p, Rect r)
    {
        return ComputeOutCode(p.X, p.Y, r);
    }

    private static Point CalculateIntersection(Rect r, Point p1, Point p2, OutCode clipTo)
    {
        double num = p2.X - p1.X;
        double num2 = p2.Y - p1.Y;
        double num3 = num / num2;
        double num4 = num2 / num;
        if (clipTo.HasFlag(OutCode.Top))
        {
            return new Point(p1.X + num3 * (r.Top - p1.Y), r.Top);
        }

        if (clipTo.HasFlag(OutCode.Bottom))
        {
            return new Point(p1.X + num3 * (r.Bottom - p1.Y), r.Bottom);
        }

        if (clipTo.HasFlag(OutCode.Right))
        {
            return new Point(r.Right, p1.Y + num4 * (r.Right - p1.X));
        }

        if (clipTo.HasFlag(OutCode.Left))
        {
            return new Point(r.Left, p1.Y + num4 * (r.Left - p1.X));
        }

        throw new ArgumentOutOfRangeException("clipTo = " + clipTo);
    }

    private static (Point, Point)? ClipSegment(Rect r, Point p1, Point p2)
    {
        OutCode outCode = ComputeOutCode(p1, r);
        OutCode outCode2 = ComputeOutCode(p2, r);
        bool flag = false;
        while (true)
        {
            if ((outCode | outCode2) == OutCode.Inside)
            {
                flag = true;
                break;
            }

            if ((outCode & outCode2) != OutCode.Inside)
            {
                break;
            }

            OutCode outCode3 = ((outCode != OutCode.Inside) ? outCode : outCode2);
            Point point = CalculateIntersection(r, p1, p2, outCode3);
            if (outCode3 == outCode)
            {
                p1 = point;
                outCode = ComputeOutCode(p1, r);
            }
            else
            {
                p2 = point;
                outCode2 = ComputeOutCode(p2, r);
            }
        }

        if (flag)
        {
            return (p1, p2);
        }

        return null;
    }

    private static Rect GetLineRect(List<Point> polyLine)
    {
        double num = double.MaxValue;
        double num2 = double.MaxValue;
        double num3 = double.MinValue;
        double num4 = double.MinValue;
        foreach (Point item in polyLine)
        {
            if (item.X < num)
            {
                num = item.X;
            }

            if (item.Y < num2)
            {
                num2 = item.Y;
            }

            if (item.X > num3)
            {
                num3 = item.X;
            }

            if (item.Y > num4)
            {
                num4 = item.Y;
            }
        }

        return new Rect(num, num2, num3 - num, num4 - num2);
    }

    /// <summary>
    /// Clips a polyline to the given bounds, writing visible segments into <paramref name="result"/>.
    /// The caller owns <paramref name="result"/> and can reuse it across calls to avoid allocations.
    /// </summary>
    /// <returns>true if any visible segments remain; false if the line is fully outside.</returns>
    public static bool ClipPolyline(List<Point> polyLine, Rect bounds, List<Point> result)
    {
        result.Clear();

        Rect lineRect = GetLineRect(polyLine);
        if (!bounds.IntersectsWith(lineRect))
            return false;

        for (var i = 1; i < polyLine.Count; i++)
        {
            var p = polyLine[i - 1];
            var p2 = polyLine[i];
            var tuple = ClipSegment(bounds, p, p2);
            if (tuple != null)
            {
                var (seg1, seg2) = tuple.Value;
                if (result.Count == 0)
                {
                    result.Add(seg1);
                    result.Add(seg2);
                }
                else if (result[^1] == seg1)
                {
                    result.Add(seg2);
                }
                else
                {
                    result.Add(seg1);
                    result.Add(seg2);
                }
            }
            else
            {
                _log.Debug("Segment is null");
            }
        }

        return result.Count > 0;
    }
}
