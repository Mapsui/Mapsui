using System;
using System.Collections.Generic;
using System.Collections;

namespace Mapsui.Fetcher
{
    public class HashSet<T> : ICollection<T>
    {
        private readonly Dictionary<T, short> _dictionary;

        public HashSet()
        {
            _dictionary = new Dictionary<T, short>();
        }

        // Methods
        public void Add(T item)
        {
            // We don't care for the value in dictionary, Keys matter.
            _dictionary.Add(item, 0);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            return _dictionary.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        // Properties
        public int Count
        {
            get { return _dictionary.Keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

}
