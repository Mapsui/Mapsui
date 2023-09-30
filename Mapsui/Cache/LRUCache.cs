using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Cache;

/// <summary>/// LRU Cache with disposing of disposable values. </summary>
public class LruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, (LinkedListNode<TKey> Node, TValue Value)> _cache;
    private readonly LinkedList<TKey> _list;
    private readonly object _lock = new object();

    public LruCache(int capacity)
    {
        _capacity = capacity;
        _cache = new(capacity);
        _list = new LinkedList<TKey>();
    }

    public void Put(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_cache.ContainsKey(key)) // Key already exists.
            {
                var node = _cache[key];
                // dispose disposable values
                if (node.Value is IDisposable disposable)
                {
#pragma warning disable IDISP007 // Don't dispose injected                    
                    disposable.Dispose();
#pragma warning restore IDISP007                    
                }

                _list.Remove(node.Node);
                _list.AddFirst(node.Node);

                _cache[key] = (node.Node, value);
            }
            else
            {
                if (_cache.Count >= _capacity) // Cache full.
                {
                    var removeKey = _list.Last!.Value;
                    _cache.TryGetValue(removeKey, out var old);
                    if (old.Value is IDisposable disposable)
                    {
#pragma warning disable IDISP007 // Don't dispose injected                    
                        disposable.Dispose();
#pragma warning restore IDISP007
                    }

                    _cache.Remove(removeKey);
                    _list.RemoveLast();
                }

                // add cache
                _cache.Add(key, (_list.AddFirst(key), value));
            }
        }
    }

    public TValue? Get(TKey key)
    {
        lock (_lock)
        {
            if (!_cache.ContainsKey(key))
            {
                return default;
            }

            var node = _cache[key];
            _list.Remove(node.Node);
            _list.AddFirst(node.Node);

            return node.Value;
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {

            if (!_cache.ContainsKey(key))
            {
                value = default;
                return false;
            }

            var node = _cache[key];
            _list.Remove(node.Node);
            _list.AddFirst(node.Node);

            value = node.Value;
            return true;
        }
    }

    [MaybeNull]
    public TValue this[TKey key]
    {
        get => Get(key);
        set => Put(key, value);
    }
}
