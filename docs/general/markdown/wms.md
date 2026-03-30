# WMS

WMS (Web Map Service) is an OGC standard for serving geo-referenced map images over HTTP. Mapsui supports WMS through the `WmsProvider` class in the `Mapsui.Extensions` package.

## Creating a WMS layer

`WmsProvider.CreateAsync` fetches the service's `GetCapabilities` response to discover available layers and coordinate systems.

```csharp
var provider = await WmsProvider.CreateAsync("https://example.com/wms");
var layer = new ImageLayer("WMS") { DataSource = provider };
map.Layers.Add(layer);
```

## Setting User-Agent and custom HTTP headers

Some WMS servers require a specific `User-Agent` or `Referer` header. `WmsProvider.CreateAsync` accepts both a `userAgent` parameter and an `httpHeaders` dictionary for any additional headers:

```csharp
var provider = await WmsProvider.CreateAsync(url,
    userAgent: "your-app-name",
    httpHeaders: new Dictionary<string, string>
    {
        ["Referer"] = "https://yoursite.com"
    });
var layer = new ImageLayer("WMS") { DataSource = provider };
map.Layers.Add(layer);
```
