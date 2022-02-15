using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IPictureRenderer : IRenderer
{
    object? RenderToPicture(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, Color? background = null);
    MemoryStream? RenderToPictureStream(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, Color? background = null);
}