using System;

namespace Mapsui.Extensions.Cache;

public class UrlCache
{
    public string Url { get; set; }
    public DateTime Created { get; set; }
    public byte[] Data { get; set; }
}