using System.IO;

namespace Mapsui.Rendering.OpenTK
{
    /// <summary>
    /// <remarks>This class is specific for the iOS platform</remarks>
    /// </summary>
    public class TextureLoader
    {
        public static void TexImage2D(Stream data, out int width, out int height)
        {
            width = 0;  //!!!bitmap.Width;
            height = 0; //!!!bitmap.Height;
        }
    }
}