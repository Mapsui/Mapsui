using System;

namespace Mapsui.Extensions.Cache;

public class UrlCache
{
    public string Url { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string Compression { get; set; } = string.Empty;
    public byte[]? Data { get; set; }
}
