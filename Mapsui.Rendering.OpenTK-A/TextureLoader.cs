using System.IO;
using Android.Graphics;
using OpenTK.Graphics.ES11;

namespace Mapsui.Rendering.OpenTK
{
    public static class TextureLoader
    {
        public static void TexImage2D(Stream data)
        {
            data.Position = 0;
            var tileArray = ReadFully(data);
            var bitmap = BitmapFactory.DecodeByteArray(tileArray, 0, tileArray.Length, new BitmapFactory.Options());
            Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, bitmap, 0);
        }

        public static byte[] ReadFully(Stream input)
        {
            using (var memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}