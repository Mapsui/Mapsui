using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mapsui.Samples.Forms;

public static class Refs
{
    private static ConcurrentQueue<WeakReference> _refs = new();

    public static void AddRef(object p)
    {
        _refs.Enqueue(new WeakReference(p));
    }

    private static void Collect()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
    }

    public static string Inspect()
    {
        Collect();
        ConcurrentQueue<WeakReference> remainingRefs = new();

        Dictionary<string, int> count = new();
        foreach (var myref in _refs)
        {
            object obj = myref.Target;
            if (obj != null)
            {
                string k = obj.GetType().FullName;
                if (count.TryGetValue(k, out int v))
                {
                    count[k] = v + 1;
                }
                else
                {
                    count[k] = 1;
                }
                remainingRefs.Enqueue(myref);
            }
        }
        string report = "Inspect refs:\n\n";
        foreach (var kv in count)
        {
            report += $"{kv.Key} = {kv.Value}\n";
        }
        _refs = remainingRefs;
        return report;
    }
}
