using Mapsui.Fetcher;
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
            foreach (var weakRef in handlers)
            {
                if (weakRef is { IsAlive: true, Target: PropertyChangedEventHandler handler })
                    handler(sender, args);
            }
        }
    }
}
