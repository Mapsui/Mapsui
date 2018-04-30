// Copyright 2010 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using Mapsui.Geometries;

namespace Mapsui
{
    public interface IViewport
    {
        /// <summary>
        /// Coordinate of center of viewport in map coordinates
        /// </summary>
        Point Center { get; set; }

        /// <summary>
        /// Resolution of the viewport in units per pixel
        /// </summary>
        /// <remarks>
        /// Resolution is Mapsuis form of zoom level. Because Mapsui is projection independent, there 
        /// aren't any zoom levels as other map libraries have. If your map has EPSG:3857 as projection
        /// and you want to calculate the zoom, you should use the following equation
        /// 
        ///     var zoom = (float)Math.Log(78271.51696401953125 / resolution, 2);
        /// </remarks>
        double Resolution { get; set; }

        /// <summary>
        /// BoundingBox of viewport in map coordinates respection Rotation
        /// </summary>
        /// <remarks>
        /// This BoundingBox is horizontally and vertically aligned, even if the viewport
        /// is rotated. So this BoundingBox perhaps contain parts, that are not visible.
        /// </remarks>
        BoundingBox Extent { get; }

        /// <summary>
        /// WindowExtend gives the four corner points of viewport in map coordinates
        /// </summary>
        /// <remarks>
        /// If viewport is rotated, this corner points are not horizontally or vertically
        /// aligned.
        /// </remarks>
        Quad WindowExtent { get; }

        /// <summary>
        /// Width of viewport in screen pixels
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Height of viewport in screen pixels
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Viewport rotation from True North (clockwise degrees)
        /// </summary>
        double Rotation { get; set; }

        /// <summary>
        /// IsRotated is true, when viewport displays map rotated
        /// </summary>
        bool IsRotated { get; }

        /// <summary>
        /// Viewport is initialized and ready to use
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Converts a point in map units to one in screen pixels, respecting rotation
        /// </summary>
        /// <param name="point">Coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreen(Point point);

        /// <summary>
        /// Converts a point in map units to one in screen pixels, not respecting rotation
        /// </summary>
        /// <param name="point">Coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreenUnrotated(Point point);

        /// <summary>
        /// Converts X/Y in map units to a point in screen pixels, respecting rotation
        /// </summary>
        /// <param name="x">X coordinate in map units</param>
        /// <param name="y">Y coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreen(double x, double y);

        /// <summary>
        /// Converts X/Y in map units to a point in screen pixels, not respecting rotation
        /// </summary>
        /// <param name="x">X coordinate in map units</param>
        /// <param name="y">Y coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreenUnrotated(double x, double y);

        /// <summary>
        /// Converts a point in screen pixels to one in map units, respecting rotation
        /// </summary>
        /// <param name="point">Coordinate in map units</param>
        /// <returns>Point in map units</returns>
        Point ScreenToWorld(Point point);

        /// <summary>
        /// Converts X/Y in screen pixels to a point in map units, respecting rotation
        /// </summary>
        /// <param name="worldPosition">Coordinate in map units</param>
        /// <returns>Point in map units</returns>
        Point ScreenToWorld(double x, double y);

        /// <summary>
        /// Moving the position of viewport to a new one
        /// </summary>
        /// <param name="screenX">New X position of point</param>
        /// <param name="screenY">New Y position of point</param>
        /// <param name="previousScreenX">Old X position of point</param>
        /// <param name="previousScreenY">Old Y position of point</param>
        /// <param name="deltaScale">Change of resolution for transformation (<1: zoom out, >1: zoom in)</param>
        /// <param name="deltaRotation">Change of rotation</param>
        void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY, 
            double deltaScale = 1, double deltaRotation = 0);
    }
}
