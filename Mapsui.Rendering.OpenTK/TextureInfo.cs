namespace Mapsui.Rendering.OpenTK
{
    /// <summary>
    /// The main purpose of this class is to store the width and height of a texture because ES11 has no api to query these parameters.
    /// </summary>
    public struct TextureInfo
    {
        public int Width;
        public int Height;
        public int TextureId;
        public long IterationUsed;
    }
}
