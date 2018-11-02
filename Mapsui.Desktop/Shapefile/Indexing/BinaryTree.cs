// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mapsui.Desktop.Shapefile.Indexing
{
    [Serializable]
    internal class Node<T, TU> where T : IComparable<T>
    {
        public BinaryTree<T, TU>.ItemValue Item;
        public Node<T, TU> LeftNode;
        public Node<T, TU> RightNode;

        public Node() : this(default(T), default(TU), null, null)
        {
        }

        public Node(T item, TU itemIndex) : this(item, itemIndex, null, null)
        {
        }

        public Node(BinaryTree<T, TU>.ItemValue value) : this(value.Value, value.Id, null, null)
        {
        }

        public Node(T item, TU itemIndex, Node<T, TU> right, Node<T, TU> left)
        {
            RightNode = right;
            LeftNode = left;
            Item = new BinaryTree<T, TU>.ItemValue
            {
                Value = item,
                Id = itemIndex
            };
        }

        public static bool operator >(Node<T, TU> lhs, Node<T, TU> rhs)
        {
            int res = lhs.Item.Value.CompareTo(rhs.Item.Value);
            return res > 0;
        }

        public static bool operator <(Node<T, TU> lhs, Node<T, TU> rhs)
        {
            int res = lhs.Item.Value.CompareTo(rhs.Item.Value);
            return res < 0;
        }
    }

    /// <summary>
    /// The BinaryTree class are used for indexing values to enhance the speed of queries
    /// </summary>
    /// <typeparam name="T">Value type to be indexed</typeparam>
    /// <typeparam name="TU">Value ID type</typeparam>
    [Serializable]
    public class BinaryTree<T, TU> where T : IComparable<T>
    {
        private readonly Node<T, TU> _root;

        /// <summary>
        /// Initializes a new instance of the generic binary tree.
        /// </summary>
        public BinaryTree()
        {
            _root = new Node<T, TU>();
        }

        /// <summary>
        /// Inserts a value into the tree
        /// </summary>
        /// <param name="items"></param>
        public void Add(params ItemValue[] items)
        {
            Array.ForEach(items, Add);
        }

        /// <summary>
        /// Inserts a value into the tree
        /// </summary>
        /// <param name="item"></param>
        public void Add(ItemValue item)
        {
            Add(new Node<T, TU>(item.Value, item.Id), _root);
        }

        /// <summary>
        /// Inserts a node into the tree
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="root"></param>
        private void Add(Node<T, TU> newNode, Node<T, TU> root)
        {
            if (newNode > root)
            {
                if (root.RightNode == null)
                {
                    root.RightNode = newNode;
                    return;
                }
                Add(newNode, root.RightNode);
            }

            if (newNode < root)
            {
                if (root.LeftNode == null)
                {
                    root.LeftNode = newNode;
                    return;
                }
                Add(newNode, root.LeftNode);
            }
        }

        /// <summary>
        /// This is the classic computer science binary tree iteration 
        /// </summary>
        public void TraceTree()
        {
            TraceInOrder(_root.RightNode);
        }

        private void TraceInOrder(Node<T, TU> root)
        {
            if (root.LeftNode != null)
                TraceInOrder(root.LeftNode);

            Trace.WriteLine(root.Item.ToString());

            if (root.RightNode != null)
                TraceInOrder(root.RightNode);
        }

        
        /// <summary>
        /// Gets an enumerator for all the values in the tree in ascending order
        /// </summary>
        public IEnumerable<ItemValue> InOrder => ScanInOrder(_root.RightNode);

        /// <summary>
        /// Gets and enumerator for the values between min and max in ascending order
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Enumerator</returns>
        public IEnumerable<ItemValue> Between(T min, T max)
        {
            return ScanBetween(min, max, _root.RightNode);
        }

        /// <summary>
        /// Enumerates the objects whose string-representation starts with 'str'
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Enumerator</returns>
        public IEnumerable<ItemValue> StartsWith(string str)
        {
            return ScanString(str.ToUpper(), _root.RightNode);
        }

        /// <summary>
        /// Enumerates all objects with the specified value
        /// </summary>
        /// <param name="value">Value to search for</param>
        /// <returns>Enumerator</returns>
        public IEnumerable<ItemValue> Find(T value)
        {
            return ScanFind(value, _root.RightNode);
        }

        private IEnumerable<ItemValue> ScanFind(T value, Node<T, TU> root)
        {
            if (root.Item.Value.CompareTo(value) > 0)
            {
                if (root.LeftNode != null)
                {
                    if (root.LeftNode.Item.Value.CompareTo(value) > 0)
                        foreach (ItemValue item in ScanFind(value, root.LeftNode))
                        {
                            yield return item;
                        }
                }
            }

            if (root.Item.Value.CompareTo(value) == 0)
                yield return root.Item;

            if (root.Item.Value.CompareTo(value) < 0)
            {
                if (root.RightNode != null)
                {
                    if (root.RightNode.Item.Value.CompareTo(value) > 0)
                        foreach (ItemValue item in ScanFind(value, root.RightNode))
                        {
                            yield return item;
                        }
                }
            }
        }

        private IEnumerable<ItemValue> ScanString(string val, Node<T, TU> root)
        {
            if (string.Compare(root.Item.Value.ToString().Substring(0, val.Length).ToUpper(), 
                val, StringComparison.Ordinal) > 0)
            {
                if (root.LeftNode != null)
                {
                    if (root.LeftNode.Item.Value.ToString().ToUpper().StartsWith(val))
                        foreach (var item in ScanString(val, root.LeftNode))
                        {
                            yield return item;
                        }
                }
            }

            if (root.Item.Value.ToString().ToUpper().StartsWith(val))
                yield return root.Item;

            if (string.Compare(root.Item.Value.ToString(), val, StringComparison.Ordinal) < 0)
            {
                if (root.RightNode != null)
                {
                    if (string.Compare(root.RightNode.Item.Value.ToString().Substring(0, val.Length).ToUpper(), val, StringComparison.Ordinal) > 0)
                        foreach (ItemValue item in ScanString(val, root.RightNode))
                        {
                            yield return item;
                        }
                }
            }
        }

        private IEnumerable<ItemValue> ScanBetween(T min, T max, Node<T, TU> root)
        {
            if (root.Item.Value.CompareTo(min) > 0)
            {
                if (root.LeftNode != null)
                {
                    if (root.LeftNode.Item.Value.CompareTo(min) > 0)
                        foreach (ItemValue item in ScanBetween(min, max, root.LeftNode))
                        {
                            yield return item;
                        }
                }
            }

            if (root.Item.Value.CompareTo(min) > 0 && root.Item.Value.CompareTo(max) < 0)
                yield return root.Item;

            if (root.Item.Value.CompareTo(max) < 0)
            {
                if (root.RightNode != null)
                {
                    if (root.RightNode.Item.Value.CompareTo(min) > 0)
                        foreach (ItemValue item in ScanBetween(min, max, root.RightNode))
                        {
                            yield return item;
                        }
                }
            }
        }

        private IEnumerable<ItemValue> ScanInOrder(Node<T, TU> root)
        {
            if (root.LeftNode != null)
            {
                foreach (ItemValue item in ScanInOrder(root.LeftNode))
                {
                    yield return item;
                }
            }

            yield return root.Item;

            if (root.RightNode != null)
            {
                foreach (ItemValue item in ScanInOrder(root.RightNode))
                {
                    yield return item;
                }
            }
        }
        
        /// <summary>
        /// A value in a <see cref="BinaryTree&lt;T, U&gt;"/>.
        /// </summary>
        public struct ItemValue
        {
            /// <summary>
            /// Identifier for the value
            /// </summary>
            public TU Id;

            /// <summary>
            /// Value
            /// </summary>
            public T Value;

            /// <summary>
            /// Creates an instance of an item in a <see cref="BinaryTree&lt;T, U&gt;"/>.
            /// </summary>
            /// <param name="value">Value</param>
            /// <param name="id">Identifier for the value</param>
            public ItemValue(T value, TU id)
            {
                Value = value;
                Id = id;
            }
        }
    }
}