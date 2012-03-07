using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections;

namespace SharpMap.Fetcher
{
    public class HashSet<T> : ICollection<T>
    {
        private Dictionary<T, short> MyDict;

        public HashSet()
        {
            MyDict = new Dictionary<T, short>();
        }

        // Methods
        public void Add(T item)
        {
            // We don't care for the value in dictionary, Keys matter.
            MyDict.Add(item, 0);
        }

        public void Clear()
        {
            MyDict.Clear();
        }

        public bool Contains(T item)
        {
            return MyDict.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            return MyDict.Remove(item);
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
            get { return MyDict.Keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

}
