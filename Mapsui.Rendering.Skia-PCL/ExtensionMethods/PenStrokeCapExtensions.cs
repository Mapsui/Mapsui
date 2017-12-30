using Mapsui.Styles;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.Rendering.Skia.ExtensionMethods
{
    public static class PenStrokeCapExtensions
    {
        public static SKStrokeCap ToSkia(this PenStrokeCap penStrokeCap)
        {
            switch (penStrokeCap)
            {
                case PenStrokeCap.Butt:
                    return SKStrokeCap.Butt;
                case PenStrokeCap.Round:
                    return SKStrokeCap.Round;
                case PenStrokeCap.Square:
                    return SKStrokeCap.Square;
                default:
                    return SKStrokeCap.Butt;
            }
        }
    }
}
