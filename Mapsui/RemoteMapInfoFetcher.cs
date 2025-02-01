using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Rendering;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Mapsui;

public class RemoteMapInfoFetcher
{
    delegate Task<IEnumerable<MapInfoRecord>> GetMapInfoAsyncDelegate();

    public static async Task<MapInfo> GetRemoteMapInfoAsync(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers)
    {
        var featureInfoLayers = layers.Where(l => l is ILayerFeatureInfo).ToList();

        var tasks = new List<Task<IEnumerable<MapInfoRecord>>>();
        var mapInfo = new MapInfo(screenPosition, viewport.ScreenToWorld(screenPosition), viewport.Resolution);

        if (!viewport.ToExtent()?.Contains(viewport.ScreenToWorld(mapInfo.ScreenPosition)) ?? false)
            return await Task.FromResult(mapInfo);

        try
        {
            var width = (int)viewport.Width;
            var height = (int)viewport.Height;

            var intX = (int)screenPosition.X;
            var intY = (int)screenPosition.Y;

            if (intX >= width || intY >= height)
                return await Task.FromResult(mapInfo);

            for (var index = 0; index < featureInfoLayers.Count; index++)
            {
                var list = new ConcurrentQueue<List<MapInfoRecord>>();
                var infoLayer = featureInfoLayers[index];
                if (infoLayer is ILayerFeatureInfo layerFeatureInfo)
                {
                    GetMapInfoAsyncDelegate getMapInfoAsync = async () =>
                    {
                        try
                        {
                            // creating new list to avoid multithreading problems
                            var mapList = new List<MapInfoRecord>();
                            // get information from ILayer Feature Info
                            var features = await layerFeatureInfo.GetFeatureInfoAsync(viewport, screenPosition);
                            foreach (var it in features)
                            {
                                foreach (var feature in it.Value)
                                {
                                    var mapInfoRecord = new MapInfoRecord(feature, infoLayer.Style!, infoLayer);
                                    mapList.Add(mapInfoRecord);
                                }
                            }

                            // atomic replace of new list is thread safe.
                            list.Enqueue(mapList);
                        }
                        catch (Exception e)
                        {
                            Logger.Log(LogLevel.Error, e.Message, e);
                        }
                        return list.SelectMany(l => l);
                    };

                    tasks.Add(getMapInfoAsync());
                }
            }

            var results = await Task.WhenAll(tasks);
            return new MapInfo(screenPosition, viewport.ScreenToWorld(screenPosition), viewport.Resolution, results.SelectMany(f => f).Reverse());
        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Error, $"Unexpected error in remote MapInfo skia renderer", exception);
        }

        return await Task.FromResult(mapInfo);
    }
}
