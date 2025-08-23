using System;

namespace Mapsui.Rendering;

[Obsolete("Use IMapRenderer instead")] // We renamed the IRenderer to IMapRenderer but preserve the IRenderer to inform the users with the obsolete warning.
public interface IRenderer : IMapRenderer
{
}
