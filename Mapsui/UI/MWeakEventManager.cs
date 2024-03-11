using System;
using System.Collections.Generic;

namespace Mapsui.UI;

public abstract class MWeakEventManager
{
    protected readonly Dictionary<object, List<WeakReference>> EventHandlers = new();

    public void AddListener(object source, Delegate? handler)
    {
        if (!EventHandlers.TryGetValue(source, out var handlers))
        {
            EventHandlers[source] = handlers = new List<WeakReference>();
        }

        handlers.Add(new WeakReference(handler));
    }

    public void RemoveListener(object source, Delegate? handler)
    {
        if (EventHandlers.TryGetValue(source, out var handlers))
        {
            handlers.RemoveAll(weakRef => !weakRef.IsAlive || Equals(weakRef.Target, handler));
        }
    }
}
