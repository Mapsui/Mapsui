using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface ILabelCache
{
    T GetOrCreateTypeface<T>(Font font, Func<Font, T> createTypeFace)
        where T : class;
    T GetOrCreateLabel<T>(string? text, LabelStyle style, float opacity, Func<LabelStyle, string?, float, ILabelCache, T> createLabelAsBitmap)
        where T : IBitmapInfo;
}
