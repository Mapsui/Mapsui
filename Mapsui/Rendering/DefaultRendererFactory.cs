using System;

namespace Mapsui.Rendering;

public static class DefaultRendererFactory
{
    private static IMapRenderer? _renderer;
    static DefaultRendererFactory()
    {
        Create = () => throw new Exception("No method to create a renderer was registered");
    }

    public static Func<IMapRenderer> Create { get; set; }
    public static IMapRenderer GetRenderer()
    {
        return _renderer ??= Create();
    }
}
