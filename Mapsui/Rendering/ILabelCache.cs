using System;
using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public interface ILabelCache
    {
        object GetOrCreateTypeface(Font font);
        IBitmapInfo GetOrCreateLabel(string? text, LabelStyle style, Func<IBitmapInfo,LabelStyle, string?, float, ILabelCache> createLabelAsBitmap);
    }
}