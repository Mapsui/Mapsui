namespace Mapsui.Styles;

/// <summary>
/// Defines which section of an image (the atlas) should be used from drawing a symbol (the sprite).
/// </summary>
/// <param name="X">The X-coordinate of the origin within the atlas.</param>
/// <param name="Y">The Y-coordinate of the origin within the atlas.</param>
/// <param name="Width">The width of the region.</param>
/// <param name="Height">The height of the region.</param>
public record BitmapRegion(int X, int Y, int Width, int Height)
{ }
