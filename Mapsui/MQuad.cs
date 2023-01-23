// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Scott Dewald as part of Mapsui

using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui;

/// <summary>
///     Double precision polygon with 4 explicit vertices. This is useful to represent
///     a MRect that has been rotated.
/// </summary>
/// <remarks>
///     The sides do not have to be parallel to the two axes of the coordinate system.
///     If this has been rotated, the 'BottomLeft' vertex may not actually be the min point in x/y.
/// </remarks>
public class MQuad : IEquatable<MQuad>
{
    public MQuad()
    {
        BottomLeft = new MPoint();
        TopLeft = new MPoint();
        TopRight = new MPoint();
        BottomRight = new MPoint();
    }

    public MQuad(MQuad quad)
    {
        BottomLeft = quad.BottomLeft;
        TopLeft = quad.TopLeft;
        TopRight = quad.TopRight;
        BottomRight = quad.BottomRight;
    }

    public MQuad(MPoint bottomLeft, MPoint topLeft, MPoint topRight, MPoint bottomRight)
    {
        BottomLeft = bottomLeft;
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
    }

    public MPoint BottomLeft { get; set; }
    public MPoint TopLeft { get; set; }
    public MPoint TopRight { get; set; }
    public MPoint BottomRight { get; set; }

    /// <summary>
    ///     Returns the vertices in clockwise order from bottom left around to bottom right
    /// </summary>
    public IEnumerable<MPoint> Vertices
    {
        get
        {
            yield return BottomLeft;
            yield return TopLeft;
            yield return TopRight;
            yield return BottomRight;
        }
    }

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">Other object to compare</param>
    /// <returns>Returns true if they are equal</returns>
    public bool Equals(MQuad? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return Equals(BottomLeft, other.BottomLeft) &&
               Equals(TopLeft, other.TopLeft) &&
               Equals(TopRight, other.TopRight) &&
               Equals(BottomRight, other.BottomRight);
    }

    /// <summary>
    ///     Creates a new quad by rotate all 4 vertices clockwise about the specified center point
    /// </summary>
    /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
    /// <param name="centerX">X coordinate of point about which to rotate</param>
    /// <param name="centerY">Y coordinate of point about which to rotate</param>
    /// <returns>Returns the rotate quad</returns>
    public MQuad Rotate(double degrees, double centerX, double centerY)
    {
        var newBottomLeft = BottomLeft.Rotate(degrees, centerX, centerY);
        var newTopLeft = TopLeft.Rotate(degrees, centerX, centerY);
        var newTopRight = TopRight.Rotate(degrees, centerX, centerY);
        var newBottomRight = BottomRight.Rotate(degrees, centerX, centerY);

        return new MQuad(newBottomLeft, newTopLeft, newTopRight, newBottomRight);
    }

    /// <summary>
    ///     Calculates a new bounding box that encompasses all 4 vertices.
    /// </summary>
    /// <returns>Returns the calculate bounding box</returns>
    public MRect ToBoundingBox()
    {
        var minX = Vertices.Select(p => p.X).Min();
        var minY = Vertices.Select(p => p.Y).Min();
        var maxX = Vertices.Select(p => p.X).Max();
        var maxY = Vertices.Select(p => p.Y).Max();

        return new MRect(minX, minY, maxX, maxY);
    }

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">Other object to compare</param>
    /// <returns>Returns true if they are equal</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as MQuad);
    }

    /// <summary>
    ///     Returns a hash code for the specified object
    /// </summary>
    /// <returns>A hash code for the specified object</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = BottomLeft != null ? BottomLeft.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (TopLeft != null ? TopLeft.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (TopRight != null ? TopRight.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (BottomRight != null ? BottomRight.GetHashCode() : 0);
            return hashCode;
        }
    }

    /// <summary>
    ///     Returns a string representation of the vertices from bottom-left clockwise to bottom-right
    /// </summary>
    /// <returns>Returns the string</returns>
    public override string ToString()
    {
        return $"BL: {BottomLeft}  TL: {TopLeft}  " +
               $"TR: {TopRight}  BR: {BottomRight}";
    }
}
