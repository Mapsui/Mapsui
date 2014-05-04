using System;
using System.Linq;

namespace Mapsui.Styles
{
    //I think this class should be replaced by doubles for Width and Height. PDD
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
