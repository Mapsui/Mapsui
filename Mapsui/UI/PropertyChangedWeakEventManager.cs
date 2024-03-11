using System.Collections.Generic;
using System;
using System.ComponentModel;

namespace Mapsui.UI;

// Custom event manager for demonstration purposes
public class PropertyChangedWeakEventManager : MWeakEventManager
{
    public void RaiseEvent(object source, PropertyChangedEventArgs args)
    {
        RaiseEvent(source, source, args);
    }

    public void RaiseEvent(object source, object? sender, PropertyChangedEventArgs args)
    {
        if (EventHandlers.TryGetValue(source, out var handlers))
        {
            List<WeakReference>? removed = null;
            foreach (var weakRef in handlers)
            {
                if (weakRef is { IsAlive: true, Target: PropertyChangedEventHandler handler })
                {
                    handler(sender, args);
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
