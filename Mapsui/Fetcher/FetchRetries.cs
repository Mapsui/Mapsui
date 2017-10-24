using System;
using System.Collections.Generic;
using BruTile;

namespace Mapsui.Fetcher
{
    /// <summary>
    /// Keeps track of retries per tile. This class doesn't do much interesting work
    /// but makes the rest of the code a bit easier to read.
    /// </summary>
    class FetchRetries
    {
        private readonly IDictionary<TileIndex, int> _retries = new Dictionary<TileIndex, int>();
        private readonly int _maxRetries;
        private readonly int _threadId;
        private const string CrossThreadExceptionMessage = "Cross thread access not allowed on class Retries";

        public FetchRetries(int maxRetries)
        {
            _maxRetries = maxRetries;
            _threadId = Environment.CurrentManagedThreadId;
        }

        public bool ReachedMax(TileIndex index)
        {
            if (_threadId != Environment.CurrentManagedThreadId) throw new Exception(CrossThreadExceptionMessage);

            var retryCount = !_retries.Keys.Contains(index) ? 0 : _retries[index];
            return retryCount > _maxRetries;
        }

        public void PlusOne(TileIndex index)
        {
            if (_threadId != Environment.CurrentManagedThreadId) throw new Exception(CrossThreadExceptionMessage);

            if (!_retries.Keys.Contains(index)) _retries.Add(index, 0);
            else _retries[index]++;
        }

        public void Clear()
        {
            if (_threadId != Environment.CurrentManagedThreadId) throw new Exception(CrossThreadExceptionMessage);

            _retries.Clear();
        }
    }
}
