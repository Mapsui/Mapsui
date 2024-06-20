using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Widgets;

namespace Mapsui.Styles;
public static class ImageSourceCacheInitializer
{
    readonly static FetchMachine _fetchMachine = new(1);

    public static void FetchImagesInViewport(ImageSourceCache imageSourceCache, Viewport viewport,
        IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, Action<bool> doneInitializing)
    {
        var imageSources = GetAllImageSources(viewport, layers);
        GetAllImageSources(imageSources, widgets);

        if (imageSources.Count == 0)
        {
            doneInitializing(false);
            return; // Don't start a thread if there are no bitmap paths to initialize.
        }

        _fetchMachine.Start(async () =>
        {
            foreach (var imageSource in imageSources)
            {
                try
                {
                    await imageSourceCache.RegisterAsync(imageSource);
                }
                catch (Exception ex)
                {
                    // Todo: We might need to deal with failed initializations, and possible reties, but not too many retries.
                    Logger.Log(LogLevel.Error, ex.Message, ex);
                }
            }
            doneInitializing(true);
        });
    }

    private static void GetAllImageSources(List<string> imageSources, IEnumerable<IWidget> widgets)
    {
        foreach (var widget in widgets)
            if (widget is IHasImageSource { ImageSource: not null } imageSource)
                imageSources.Add(imageSource.ImageSource);
    }

    public static async Task<bool> FetchImagesInViewportAsync(ImageSourceCache imageSourceCache,
        Viewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget>? widgets = null)
    {
        var imageSources = GetAllImageSources(viewport, layers);
        if (widgets != null)
            GetAllImageSources(imageSources, widgets);   

        if (imageSources.Count == 0)
            return await Task.FromResult(false);

        foreach (var imageSource in imageSources)
        {
            try
            {
                await imageSourceCache.RegisterAsync(imageSource);
            }
            catch (Exception ex)
            {
                // Todo: We might need to deal with failed initializations, and possible reties, but not too many retries.
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }
        return await Task.FromResult(true);
    }

    private static List<string> GetAllImageSources(Viewport viewport, IEnumerable<ILayer> layers)
    {
        var result = new List<string>();
        VisibleFeatureIterator.IterateLayers(viewport, layers, 0, (v, l, s, f, o, i) =>
        {
            // Get ImageSource directly from Styles
            if (s is IHasImageSource imageSource)
            {
                if (imageSource.ImageSource is not null)
                    result.Add(imageSource.ImageSource);
            }

            // Get ImageSource from Brushes
            if (s is SymbolStyle symbolStyle)
            {
                if (symbolStyle.Fill is IHasImageSource fillImageSource)
                    if (fillImageSource.ImageSource is not null)
                        result.Add(fillImageSource.ImageSource);
            }
            else if (s is VectorStyle vectorStyle)
            {
                if (vectorStyle.Fill is IHasImageSource fillImageSource)
                    if (fillImageSource.ImageSource is not null)
                        result.Add(fillImageSource.ImageSource);
            }
        });
        return result;
    }
}
