using System;
using System.Collections.Generic;
using Mapsui.Fetcher;

namespace Mapsui.UI;

// Custom event manager for demonstration purposes
public class DataChangedWeakEventManager : MWeakEventManager
{
    public void RaiseEvent(object source, DataChangedEventArgs args)
    {
        if (EventHandlers.TryGetValue(source, out var handlers))
        {
            List<WeakReference>? removed = null;
            foreach (var weakRef in handlers)
            {
                if (weakRef is { IsAlive: true, Target: DataChangedEventHandler handler })
                {
                    handler(source, args);
                }
                else
                {
                    removed ??= new();
                    removed.Add(weakRef);
                }
            }

            if (removed != null)
            {
                foreach (var weakRef in removed)
                    handlers.Remove(weakRef);
            }
        }
    }
}
