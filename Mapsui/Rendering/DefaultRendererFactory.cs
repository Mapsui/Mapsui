using System;

namespace Mapsui.Rendering;

public static class DefaultRendererFactory
{
    private static Func<IMapRenderer> _create = () => throw new Exception("No method to create a renderer was registered");
    private static IMapRenderer? _renderer;

    /// <summary>
    /// True once <see cref="Create"/> has been explicitly assigned.
    /// Platform renderer static constructors check this before registering themselves
    /// as the default factory, so an explicit assignment (e.g. from
    /// <c>SampleConfiguration.ApplyRendererConfig()</c>) is never overwritten.
    /// </summary>
    public static bool IsConfigured { get; private set; }

    public static Func<IMapRenderer> Create
    {
        get => _create;
        set
        {
            _create = value;
            _renderer = null; // Reset the shared singleton so the next GetRenderer() call uses the new factory.
            IsConfigured = true;
        }
    }

    public static IMapRenderer GetRenderer() => _renderer ??= _create();
}
