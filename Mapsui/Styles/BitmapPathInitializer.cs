using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Styles;
public static class BitmapPathInitializer
{
    static readonly ConcurrentHashSet<Uri> _register = [];
    static readonly object _lockObject = new();
    static readonly FetchMachine _fetchMachine = new(1);

    public static void Add(Uri bitmapPath)
    {
        lock (_lockObject)
        {
            _register.Add(bitmapPath);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doneInitializing"></param>
    public static void InitializeWhenNeeded(Action<bool> doneInitializing)
    {
        var bitmapPaths = GetAndClear();
        if (!bitmapPaths.Any())
        {
            doneInitializing(false);
            return; // Don't start a thread if there are no bitmap paths to initialize.
        }

        _fetchMachine.Start(async () =>
        {
            foreach (var bitmapPath in bitmapPaths)
            {
                try
                {
                    await ImagePathCache.Instance.RegisterAsync(bitmapPath);
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

    private static IEnumerable<Uri> GetAndClear()
    {
        lock (_lockObject)
        {
            if (_register.Count == 0)
                return Array.Empty<Uri>();
            var result = _register.ToArray();
            _register.Clear();
            return result;
        }
    }
}
