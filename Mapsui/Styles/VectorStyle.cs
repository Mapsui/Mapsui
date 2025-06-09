namespace Mapsui.Styles;

public class VectorStyle : BaseStyle
{
    public VectorStyle()
    {
        Outline = new Pen { Color = Color.Gray, Width = 1 };
        Line = new Pen { Color = Color.Black, Width = 1 };
        Fill = new Brush { Color = Color.White };
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
