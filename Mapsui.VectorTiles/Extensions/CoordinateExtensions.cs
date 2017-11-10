using System;
using Mapbox.Vector.Tile;
using Mapsui.Geometries;
using Mapsui.Projection;

namespace Mapsui.VectorTiles.Extensions
{
    public static class CoordinateExtensions
    {
        public static Point ToPosition(this Coordinate c, int x, int y, int z, uint extent)
        {
            // todo: convert directly to spherical, not to LonLat first, or just use LonLat.

            var size = extent * Math.Pow(2, z);
            var x0 = extent * x;
            var y0 = extent * y;

            var y2 = 180 - (c.Y + y0) * 360 / size;
            var lon = (c.X + x0) * 360 / size - 180;
            var lat = 360 / Math.PI * Math.Atan(Math.Exp(y2 * Math.PI / 180)) - 90;
            
            return SphericalMercator.FromLonLat(lon, lat);
        }
    }
}
