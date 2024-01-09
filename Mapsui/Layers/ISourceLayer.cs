namespace Mapsui.Layers;

/// <summary>
/// Interface for layers, that contain an underlaying source layer, which 
/// provides the real data 
/// </summary>
public interface ISourceLayer
{
    /// <summary>
    /// Layer with the real source for this layer
    /// </summary>
    ILayer SourceLayer { get; }
}
