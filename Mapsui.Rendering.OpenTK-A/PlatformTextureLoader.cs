using System.IO;
using Android.Graphics;
using OpenTK.Graphics.ES20;
using PixelFormat = OpenTK.Graphics.ES20.PixelFormat;

namespace Mapsui.Rendering.OpenTK
{
    /// <summary>
    /// <remarks>This class is specific for the Android platform</remarks>
    /// </summary>
    public static class PlatformTextureLoader
    {
        public static void TexImage2D(Stream data, out int width, out int height)
        {
            data.Position = 0;

            var bitmap = BitmapFactory.DecodeStream(data);
            // The texture that is loaded below is attached to the TextureID that was bound in an earlier call with 'GL.BindTexture' 
            bitmap.LockPixels();

            var pixels = new int[bitmap.Width * bitmap.Height];
            bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);

            // What I would like to do is to pass PixelFormat.Bgra to GL.TexImage2D but this is not available in ES20.
            // What I do below is convert the bits myself. There must be a really easy way to do this. And probably a 
            // more performing way. 
            FromBgraToRgba(pixels);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            bitmap.UnlockPixels();
            width = bitmap.Width;
            height = bitmap.Height;
            bitmap.Dispose();
        }

        private static void FromBgraToRgba(int[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = FromBgraToRgba(pixels[i]);
            }
        }

        public static int FromBgraToRgba(int pixel)
        {
            var b1 = (pixel >> 0) & 0xff;
            var b2 = (pixel >> 8) & 0xff;
            var b3 = (pixel >> 16) & 0xff;
            var b4 = (pixel >> 24) & 0xff;

            return b1 << 16 | b2 << 8 | b3 << 0 | b4 << 24;
        } 
    }
}