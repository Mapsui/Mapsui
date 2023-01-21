using System;

namespace Mapsui.Rendering;

public static class DefaultRendererFactory
{
    static DefaultRendererFactory()
    {
        Create = () => throw new Exception("No method to create a renderer was registered");
    }

    public static Func<IRenderer> Create { get; set; }
}
