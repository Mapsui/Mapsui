using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderService : IRenderCache, IDisposable
{
    IBitmapRegistry BitmapRegistry { get; }
}
