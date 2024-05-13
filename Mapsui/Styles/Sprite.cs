namespace Mapsui.Styles;

/// <summary>
/// Defines which section of the atlas (the source image) should be used as sprite image.
/// </summary>
/// <param name="X">The X-coordinate of the origin within the atlas.</param>
/// <param name="Y">The Y-coordinate of the origin within the atlas.</param>
/// <param name="Width">The width of the sprite.</param>
/// <param name="Height">The height of the sprite.</param>
public record Sprite(int X, int Y, int Width, int Height)
{ }
