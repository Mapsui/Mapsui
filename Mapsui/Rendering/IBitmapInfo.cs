using System;

namespace Mapsui.Rendering;

public interface IBitmapInfo : IDisposable
{
    long IterationUsed { get; set; }
}
