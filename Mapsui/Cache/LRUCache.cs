using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Extensions;

namespace Mapsui.Cache;

/// <summary>/// LRU Cache with disposing of disposable values. </summary>
public class LruCache<TKey, TValue>(int capacity)
    where TKey : notnull
{
    private readonly Dictionary<TKey, (LinkedListNode<TKey> Node, TValue Value)> _cache = new(capacity);
    private readonly LinkedList<TKey> _list = new();
    private readonly object _lock = new();

    public void Put(TKey key, TValue value)
    {
        lock (_lock)
        {
            InternalPut(key, value);
        }
    }

    private void InternalPut(TKey key, TValue value)
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
            if (_cache.Count >= capacity) // Cache full.
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

    public TResult? GetOrCreateValue<TParam, TResult>(TParam key, Func<TParam, TResult> func)
        where TParam : TKey
        where TResult : TValue
    {
        lock (_lock)
        {
            if (!InternalTryGetValue(key, out var value))
            {
                value = func(key);
                InternalPut(key, value);
            }

            return (TResult?)value;
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            return InternalTryGetValue(key, out value);
        }
    }

    private bool InternalTryGetValue(TKey key, out TValue? value)
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

    public void Clear()
    {
        lock (_lock)
        {
            foreach (var value in _cache.Values)
            {
                value.Value.DisposeIfDisposable();
            }

            _cache.Clear();
            _list.Clear();
        }
    }
}
