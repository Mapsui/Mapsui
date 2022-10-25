using System;
using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public interface ILabelCache
    {
        object GetOrCreateTypeface(Font font);
        IBitmapInfo GetOrCreateLabel(string? text, LabelStyle style, float opacity, Func<LabelStyle, string?, float, ILabelCache, IBitmapInfo> createLabelAsBitmap);
    }
}