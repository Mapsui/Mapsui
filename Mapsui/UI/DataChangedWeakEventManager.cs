using Mapsui.Fetcher;
using System.ComponentModel;

namespace Mapsui.UI;

// Custom event manager for demonstration purposes
public class DataChangedWeakEventManager : MWeakEventManager
{
    public void RaiseEvent(object source, DataChangedEventArgs args)
    {
        if (EventHandlers.TryGetValue(source, out var handlers))
        {
            foreach (var weakRef in handlers)
            {
                if (weakRef is { IsAlive: true, Target: DataChangedEventHandler handler })
                    handler(source, args);
            }
        }
    }
}
