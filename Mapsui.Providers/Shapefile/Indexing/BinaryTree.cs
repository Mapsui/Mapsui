// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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

namespace SharpMap.Utilities.Indexing
{
// Binary Tree not working yet on Mono 
// see bug: http://bugzilla.ximian.com/show_bug.cgi?id=78502
#if !MONO
    [Serializable]
    internal class Node<T, U> where T : IComparable<T>
    {
        public BinaryTree<T, U>.ItemValue Item;
        public Node<T, U> LeftNode;
        public Node<T, U> RightNode;

        public Node() : this(default(T), default(U), null, null)
        {
        }

        public Node(T item, U itemIndex) : this(item, itemIndex, null, null)
        {
        }

        public Node(BinaryTree<T, U>.ItemValue value) : this(value.Value, value.Id, null, null)
        {
        }

        public Node(T item, U itemIndex, Node<T, U> right, Node<T, U> left)
        {
            RightNode = right;
            LeftNode = left;
            Item = new BinaryTree<T, U>.ItemValue();
            Item.Value = item;
            Item.Id = itemIndex;
        }

        public static bool operator >(Node<T, U> lhs, Node<T, U> rhs)
        {
            int res = lhs.Item.Value.CompareTo(rhs.Item.Value);
            return res > 0;
        }

        public static bool operator <(Node<T, U> lhs, Node<T, U> rhs)
        {
            int res = lhs.Item.Value.CompareTo(rhs.Item.Value);
            return res < 0;
        }
    }

    /// <summary>
    /// The BinaryTree class are used for indexing values to enhance the speed of queries
    /// </summary>
    /// <typeparam name="T">Value type to be indexed</typeparam>
    /// <typeparam name="U">Value ID type</typeparam>
    [Serializable]
    public class BinaryTree<T, U> where T : IComparable<T>
    {
        private readonly Node<T, U> root;

        /// <summary>
        /// Initializes a new instance of the generic binary tree.
        /// </summary>
        public BinaryTree()
        {
            root = new Node<T, U>();
        }

        #region Read/Write index to/from a file

        /*
		private const double INDEXFILEVERSION = 1.0;

		public void SaveToFile(string filename)
		{
			System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
			System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
			bw.Write(INDEXFILEVERSION); //Save index version
			bw.Write(typeof(T).ToString());
			bw.Write(typeof(U).ToString());
			//SaveNode(this, ref bw);
			//bw.Write(root);
			bw.Close();
			fs.Close();
		}
		*/

        #endregion

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
            Add(new Node<T, U>(item.Value, item.Id), root);
        }

        /// <summary>
        /// Inserts a node into the tree
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="root"></param>
        private void Add(Node<T, U> newNode, Node<T, U> root)
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
            TraceInOrder(root.RightNode);
        }

        private void TraceInOrder(Node<T, U> root)
        {
            if (root.LeftNode != null)
                TraceInOrder(root.LeftNode);

            Trace.WriteLine(root.Item.ToString());

            if (root.RightNode != null)
                TraceInOrder(root.RightNode);
        }

        #region IEnumerables

        /// <summary>
        /// Gets an enumerator for all the values in the tree in ascending order
        /// </summary>
        public IEnumerable<ItemValue> InOrder
        {
            get { return ScanInOrder(root.RightNode); }
        }

        /// <summary>
        /// Gets and enumerator for the values between min and max in ascending order
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Enumerator</returns>
        public IEnumerable<ItemValue> Between(T min, T max)
        {
            return ScanBetween(min, max, root.RightNode);
        }

        /// <summary>
        /// Enumerates the objects whose string-representation starts with 'str'
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Enumerator</returns>
        public IEnumerable<ItemValue> StartsWith(string str)
        {
            return ScanString(str.ToUpper(), root.RightNode);
        }

        /// <summary>
        /// Enumerates all objects with the specified value
        /// </summary>
        /// <param name="value">Value to search for</param>
        /// <returns>Enumerator</returns>
        public IEnumerable<ItemValue> Find(T value)
        {
            return ScanFind(value, root.RightNode);
        }

        private IEnumerable<ItemValue> ScanFind(T value, Node<T, U> root)
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

        private IEnumerable<ItemValue> ScanString(string val, Node<T, U> root)
        {
            if (root.Item.Value.ToString().Substring(0, val.Length).ToUpper().CompareTo(val) > 0)
            {
                if (root.LeftNode != null)
                {
                    if (root.LeftNode.Item.Value.ToString().ToUpper().StartsWith(val))
                        foreach (ItemValue item in ScanString(val, root.LeftNode))
                        {
                            yield return item;
                        }
                }
            }

            if (root.Item.Value.ToString().ToUpper().StartsWith(val))
                yield return root.Item;

            if (root.Item.Value.ToString().CompareTo(val) < 0)
            {
                if (root.RightNode != null)
                {
                    if (root.RightNode.Item.Value.ToString().Substring(0, val.Length).ToUpper().CompareTo(val) > 0)
                        foreach (ItemValue item in ScanString(val, root.RightNode))
                        {
                            yield return item;
                        }
                }
            }
        }

        private IEnumerable<ItemValue> ScanBetween(T min, T max, Node<T, U> root)
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

        private IEnumerable<ItemValue> ScanInOrder(Node<T, U> root)
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

        #endregion

        #region Nested type: ItemValue

        /// <summary>
        /// A value in a <see cref="BinaryTree&lt;T, U&gt;"/>.
        /// </summary>
        public struct ItemValue
        {
            /// <summary>
            /// Identifier for the value
            /// </summary>
            public U Id;

            /// <summary>
            /// Value
            /// </summary>
            public T Value;

            /// <summary>
            /// Creates an instance of an item in a <see cref="BinaryTree&lt;T, U&gt;"/>.
            /// </summary>
            /// <param name="value">Value</param>
            /// <param name="id">Identifier for the value</param>
            public ItemValue(T value, U id)
            {
                Value = value;
                Id = id;
            }
        }

        #endregion
    }
#endif
}