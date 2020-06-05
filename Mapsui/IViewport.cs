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
    public interface IViewport : IReadOnlyViewport
    {
        void SetCenter(double x, double y);
        void SetCenter(ReadOnlyPoint center);
        void SetResolution(double resolution);
        void SetRotation(double rotation);
        void SetSize(double width, double height);

        /// <summary>
        /// Moving the position of viewport to a new one
        /// </summary>
        /// <param name="position">Current center of touch</param>
        /// <param name="previousPosition">Previous center of touch</param>
        /// <param name="deltaResolution">Change of resolution for transformation (&lt;1: zoom out, >1: zoom in)</param>
        /// <param name="deltaRotation">Change of rotation</param>
        void Transform(Point position, Point previousPosition, double deltaResolution = 1, double deltaRotation = 0);
    }
}
