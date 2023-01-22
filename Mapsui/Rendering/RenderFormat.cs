namespace Mapsui.Rendering;

/// <summary> Formats the MapRenderer should render to </summary>
public enum RenderFormat
{
    ///<summary>Default Format works everywhere</summary> 
    Png,
    ///<summary>20 - 30% smaller lossless Format</summary>
    WebP,
    ///<summary>Skia Vector Format</summary>
    Skp
}
