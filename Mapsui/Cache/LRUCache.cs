using System;
using System.Collections;
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

    public LruCache(int capacity)
    {
        _capacity = capacity;
        _cache = new(capacity);
        _list = new LinkedList<TKey>();
    }

    public void Put(TKey key, TValue value)
    {
        if (_cache.ContainsKey(key)) // Key already exists.
        {
            var node = _cache[key];
            _list.Remove(node.Node);
            _list.AddFirst(node.Node);
            if (!object.ReferenceEquals(node.Value, value))
            {
                // dispose disposable values
                if (node.Value is IDisposable disposable)
                {
#pragma warning disable IDISP007 // Don't dispose injected                    
                    disposable.Dispose();
#pragma warning restore IDISP007                    
                }
            }

            _cache[key] = (node.Node, value);
        }
        else
        {
            if (_cache.Count >= _capacity) // Cache full.
            {
                var removeKey = _list.Last!.Value;
                _cache.TryGetValue(removeKey, out var old);
                _cache.Remove(removeKey);
                _list.RemoveLast();

                // dispose disposable values
                if (old.Value is IDisposable disposable)
                {
#pragma warning disable IDISP007 // Don't dispose injected                    
                    disposable.Dispose();
#pragma warning restore IDISP007
                }
            }

            // add cache
            _cache.Add(key, (_list.AddFirst(key), value));
        }
    }

    public TValue? Get(TKey key)
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

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
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

    [MaybeNull]
    public TValue this[TKey key]
    {
        get => Get(key);
        set => Put(key, value);
    }
}
