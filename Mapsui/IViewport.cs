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
        Point WorldToScreen(Point point);
        Point WorldToScreenUnrotated(Point point);
        Point ScreenToWorld(Point point);
        Point WorldToScreen(double x, double y);
        Point WorldToScreenUnrotated(double x, double y);
        Point ScreenToWorld(double x, double y);
        void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY, 
            double deltaScale = 1, double deltaRotation = 0);
        Point Center { get; set; }
        double Resolution { get; set; }
        BoundingBox Extent { get; }
        Quad WindowExtent { get; }
        double Width { get; set; }
        double Height { get; set; }

        /// <summary>
        /// Viewport rotation from True North (clockwise degrees)
        /// </summary>
        double Rotation { get; set; }
        bool IsRotated { get; }
        bool Initialized { get; }
    }
}
