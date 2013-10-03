using Mapsui.Geometries;
using Microsoft.Xna.Framework.Graphics;

namespace Mapsui.Rendering.MonoGame
{
    public class GeometryRenderer
    {
        public static Texture2D RenderRaster(GraphicsDevice graphicsDevice, IRaster raster)
        {
            raster.Data.Position = 0;
            return Texture2D.FromStream(graphicsDevice, raster.Data);
        }
    }
}
