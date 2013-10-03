using Microsoft.Xna.Framework;
using BoundingBox = Mapsui.Geometries.BoundingBox;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.MonoGame
{
    public static class GeometryExtensions
    {
        public static Vector2 ToXna(this Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
        
        public static Rectangle ToXna(this BoundingBox boundingBox)
        {
            return new Rectangle
            {
                X = (int)boundingBox.Left,
                Y = (int)boundingBox.Bottom,
                Width = (int)boundingBox.Width,
                Height = (int)boundingBox.Height
            };
        }
    }
}
