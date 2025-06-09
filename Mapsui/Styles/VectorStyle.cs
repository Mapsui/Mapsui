namespace Mapsui.Styles;

public class VectorStyle : BaseStyle
{
    public VectorStyle()
    {
    }

    /// <summary>
    /// Line style for line geometries
    /// </summary>
    public Pen? Line { get; set; }

    /// <summary>
    /// Outline style for line and polygon geometries
    /// </summary>
    public Pen? Outline { get; set; }

    /// <summary>
    /// Fill style for Polygon geometries
    /// </summary>
    public Brush? Fill { get; set; }
}
