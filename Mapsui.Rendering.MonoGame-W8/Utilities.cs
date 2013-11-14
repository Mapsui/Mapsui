using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Mapsui.Rendering.MonoGame
{
    public static class Utilities
    {
        public static MemoryStream ToBitmapStream(GraphicsDevice graphicsDevice, 
            double width, double height)
        {
            var renderTarget = new RenderTarget2D(graphicsDevice, (int)width, (int)height);
            graphicsDevice.SetRenderTarget(renderTarget);

            graphicsDevice.SetRenderTarget(null);
            var stream = new MemoryStream();
            renderTarget.SaveAsPng(stream, (int)width, (int)height);
            return stream;
        }
    }
}
