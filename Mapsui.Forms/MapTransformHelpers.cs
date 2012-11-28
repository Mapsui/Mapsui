// Copyright 2009 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using SharpMap;
using Point = SharpMap.Geometries.Point;

namespace Mapsui.Forms
{
  public static class MapTransformHelpers
  {
    public static void Pan(Viewport transform, Point currentMap, Point previousMap)
    {
      Point current = transform.ScreenToWorld(currentMap.X, currentMap.Y);
      Point previous = transform.ScreenToWorld(previousMap.X, previousMap.Y);
      double diffX = previous.X - current.X;
      double diffY = previous.Y - current.Y;
      transform.Center = new Point(transform.CenterX + diffX, transform.CenterY + diffY);
    }
  }
}
