using Android.Graphics;
using OpenTK.Graphics.ES11;
using System.IO;

namespace Mapsui.Rendering.OpenTK
{
    public static class TextureLoader
    {
        public static void TexImage2D(Stream data, out int width, out int height)
        {
            data.Position = 0;
            var bitmap = BitmapFactory.DecodeStream(data);
            // The texture that is loaded below is attached to the TextureID that was bound with GL.BindTexture 
            Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, bitmap, 0);
            width = bitmap.Width;
            height = bitmap.Height;
        }
    }
}