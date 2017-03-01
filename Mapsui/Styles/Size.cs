namespace Mapsui.Styles
{
    public class Size
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public Size() {}

        public Size(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }
    }
}
