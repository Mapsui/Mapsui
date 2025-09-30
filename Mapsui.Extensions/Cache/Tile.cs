using System;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Extensions.Cache;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class Tile
{
    public int Level { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
    public DateTime Created { get; set; }
    public string Compression { get; set; } = string.Empty;
    public byte[]? Data { get; set; }
}
