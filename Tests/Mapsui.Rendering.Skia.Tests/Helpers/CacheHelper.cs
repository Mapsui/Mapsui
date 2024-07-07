// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Providers.Wfs;
using Mapsui.Providers.Wms;
using Mapsui.Samples.Common.Maps.DataFormats;
using Mapsui.Samples.Common.Maps.WMTS;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Tiling;

namespace Mapsui.Rendering.Skia.Tests.Helpers;
internal static class CacheHelper
{
    /// <summary> Initializes the caches for the samples. </summary>
    public static void InitCaches()
    {
        // Tile Cache
        OpenStreetMap.DefaultCache ??= File.ReadFromCacheFolder("OpenStreetMap");
        BingArial.DefaultCache ??= File.ReadFromCacheFolder("BingArial");
        BingHybrid.DefaultCache ??= File.ReadFromCacheFolder("BingHybrid");
        Michelin.DefaultCache ??= File.ReadFromCacheFolder("Michelin");
        TiledWmsSample.DefaultCache ??= File.ReadFromCacheFolder("TiledWmsSample");
        TmsSample.DefaultCache ??= File.ReadFromCacheFolder("TmsSample");
        WmtsSample.DefaultCache ??= File.ReadFromCacheFolder("WmtsSample");
        WmtsZurichSample.DefaultCache ??= File.ReadFromCacheFolder("WmtsZurichSample");

        // Url Cache
        WmsProvider.DefaultCache ??= File.ReadFromCacheFolder("WmsSample");
        WFSProvider.DefaultCache ??= File.ReadFromCacheFolder("WfsSample");
        ArcGISImageServiceSample.DefaultCache ??= File.ReadFromCacheFolder("ArcGisImageServiceSample");
        ArcGISDynamicServiceSample.DefaultCache ??= File.ReadFromCacheFolder("ArcGisImageServiceSample");
    }

    /// <summary> Nullifies the caches for the samples. </summary>
    public static void NullifyCaches()
    {
        // Tile Cache
        OpenStreetMap.DefaultCache = null;
        BingArial.DefaultCache = null;
        BingHybrid.DefaultCache = null;
        Michelin.DefaultCache = null;
        TiledWmsSample.DefaultCache = null;
        TmsSample.DefaultCache = null;
        WmtsZurichSample.DefaultCache = null;

        // Url Cache
        WmsProvider.DefaultCache = null;
        WFSProvider.DefaultCache = null;
        ArcGISImageServiceSample.DefaultCache = null;
        ArcGISDynamicServiceSample.DefaultCache = null;
    }
}
