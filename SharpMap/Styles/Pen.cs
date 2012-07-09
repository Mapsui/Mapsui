using System;
using System.Linq;

namespace SharpMap.Styles
{
    public class Pen
    {
        public Pen()
        {
            Width = 1;
        }

        public double Width { get; set; }
        public Color Color { get; set; }
    }
}
