using System;
using System.Collections.Generic;
using System.Collections;

namespace SharpMap.Fetcher
{
    public class HashSet<T> : ICollection<T>
    {
        private Dictionary<T, short> dictionary;

        public HashSet()
        {
            dictionary = new Dictionary<T, short>();
        }

        // Methods
        public void Add(T item)
        {
            // We don't care for the value in dictionary, Keys matter.
            dictionary.Add(item, 0);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            return dictionary.Remove(item);
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
            get { return dictionary.Keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

}
