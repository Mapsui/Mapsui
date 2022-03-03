namespace Mapsui.Samples.Common.Desktop.Cache;

public class Tile
{
    public int Level { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
    public byte[]? Data { get; set; }
}