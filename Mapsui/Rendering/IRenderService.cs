using System;

namespace Mapsui.Rendering;

public interface IRenderService : IDisposable
{
    IVectorCache VectorCache { get; }
}
