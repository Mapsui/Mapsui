using System;
using System.Collections.Generic;

namespace Mapsui.Cache;

public class LruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue?> _valueCache;
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _nodeCache;
    private readonly LinkedList<TKey> _orderList;

    public LruCache(int capacity)
    {
        _capacity = capacity;
        _valueCache = new Dictionary<TKey, TValue?>(capacity);
        _nodeCache = new Dictionary<TKey, LinkedListNode<TKey>>(capacity);
        _orderList = new LinkedList<TKey>();
    }

    public void Put(TKey key, TValue? value)
    {
        if (_valueCache.ContainsKey(key)) // Key already exists.
        {
            Promote(key);            
            _valueCache[key] = value;
            return;
        }

        if (_valueCache.Count == _capacity) // Cache full.
        {
            RemoveLast();
        }
        
        AddFirst(key, value);
    }

    public TValue? Get(TKey key)
    {
        if (!_valueCache.ContainsKey(key))
        {
            return default;
        }

        Promote(key);            
        return _valueCache[key];
    }
    
    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (!_valueCache.ContainsKey(key))
        {
            value = default;
            return false;
        }

        Promote(key);            
        return _valueCache.TryGetValue(key, out value);
    }
    
    public TValue? this[TKey key]
    {
        get => Get(key);
        set => Put(key, value);
    }

    private void AddFirst(TKey key, TValue? value)
    {
        var node = new LinkedListNode<TKey>(key);
        _valueCache[key] = value;
        _nodeCache[key] = node;
        _orderList.AddFirst(node);
    }
    
    private void Promote(TKey key)
    {
        LinkedListNode<TKey> node = _nodeCache[key];
        _orderList.Remove(node);
        _orderList.AddFirst(node);
    }
    
    private void RemoveLast()
    {
        LinkedListNode<TKey> lastNode = _orderList.Last;
        _valueCache.TryGetValue(lastNode.Value, out var value);
        _valueCache.Remove(lastNode.Value);
        _nodeCache.Remove(lastNode.Value);
        _orderList.RemoveLast();
        
        // dispose disposable values
        if (value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}