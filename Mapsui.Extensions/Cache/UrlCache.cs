using System;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Extensions.Cache;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class UrlCache
{
    public string Url { get; set; } = string.Empty;
    public byte[]? PostData { get; set; }
    public DateTime Created { get; set; }
    public string Compression { get; set; } = string.Empty;
    public byte[]? Data { get; set; }
}
