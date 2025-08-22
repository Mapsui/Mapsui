using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering;

// We renamed the IRenderer to IMapRenderer but keep the IRenderer for compatibility.
public interface IRenderer : IMapRenderer
{
}
