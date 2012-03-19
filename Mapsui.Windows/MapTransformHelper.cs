// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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

using System.Windows;
using SharpMap;

namespace Mapsui.Windows
{
    public static class MapTransformHelper
    {
        public static void Pan(View view, Point currentMap, Point previousMap)
        {
            SharpMap.Geometries.Point current = view.ViewToWorld(currentMap.X, currentMap.Y);
            SharpMap.Geometries.Point previous = view.ViewToWorld(previousMap.X, previousMap.Y);
            double diffX = previous.X - current.X;
            double diffY = previous.Y - current.Y;
            view.Center = new SharpMap.Geometries.Point(view.CenterX + diffX, view.CenterY + diffY);
        }
    }
}
