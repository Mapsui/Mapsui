using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;
public static class SkPaintExtensions
{
    public static bool IsVisible(this SKPaint? paint)
    {
        return paint != null && paint.Color.Alpha != 0;
    }
}
