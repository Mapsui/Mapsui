namespace Mapsui.Styles
{
    public class Font
    {
        public Font(){}

        public Font(Font font)
        {
            FontFamily = new string(font.FontFamily.ToCharArray());
            Size = font.Size;
        }

        public string FontFamily { get; set; }
        public double Size { get; set; }
    }
}