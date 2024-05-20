using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Styles;
public static class ImageSourceInitializer
{
    static readonly ConcurrentHashSet<string> _register = [];
    static readonly object _lockObject = new();
    static readonly FetchMachine _fetchMachine = new(1);

    public static void Add(string imageSource)
    {
        lock (_lockObject)
        {
            _register.Add(imageSource);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doneInitializing"></param>
    public static void InitializeWhenNeeded(Action<bool> doneInitializing)
    {
        var imageSources = GetAndClear();
        if (!imageSources.Any())
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
                    await ImageSourceCache.Instance.RegisterAsync(imageSource);
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

    private static IEnumerable<string> GetAndClear()
    {
        lock (_lockObject)
        {
            if (_register.Count == 0)
                return [];
            var result = _register.ToArray();
            _register.Clear();
            return result;
        }
    }
}
