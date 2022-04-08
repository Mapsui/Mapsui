using System;

namespace Mapsui.Extensions.Cache;

public class UrlCache
{
    public string Url { get; set; } = default!;
    public DateTime Created { get; set; }
    public byte[] Data { get; set; } = default!;
}