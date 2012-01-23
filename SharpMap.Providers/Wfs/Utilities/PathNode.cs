// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Xml;

namespace SharpMap.Utilities.Wfs
{
    internal interface IPathNode
    {
        bool IsActive { get; set; }
        bool Matches(XmlReader reader);
    }

    /// <summary>
    /// This class represents an element-node in an XML document 
    /// </summary>
    internal class PathNode : IPathNode
    {
        #region Fields and Properties

        private readonly string _XmlElementNodeName;
        private readonly string _XmlElementNsUri;
        private bool _IsActive = true;

        /// <summary>
        /// Gets the namespace URI of the element-node
        /// </summary>
        internal string XmlElementNsUri
        {
            get { return _XmlElementNsUri; }
        }

        /// <summary>
        /// Gets the local name of the element-node
        /// </summary>
        internal string XmlElementNodeName
        {
            get { return _XmlElementNodeName; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PathNode"/> class.
        /// </summary>
        /// <param name="elementNsUri">The namespace URI of the element-node</param>
        /// <param name="elementNodeName">The local name of the element-node</param>
        /// <param name="nameTable">A NameTable for storing namespace URI and local name</param>
        internal PathNode(string elementNsUri, string elementNodeName, NameTable nameTable)
        {
            _XmlElementNsUri = nameTable.Add(elementNsUri);
            _XmlElementNodeName = nameTable.Add(elementNodeName);
        }

        #endregion

        #region IPathNode Member

        /// <summary>
        /// This method evaluates, if the position of an XmlReader is at the element-node represented by the instance of this class.
        /// It compares pointers instead of literal values due to performance reasons.
        /// Therefore the name table of the XmlReader given as argument must be the one handed over to the constructor.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        public bool Matches(XmlReader xmlReader)
        {
            if (!_IsActive) return true;

            //Compare pointers instead of literal values
            if ((ReferenceEquals(_XmlElementNsUri, xmlReader.NameTable.Get(xmlReader.NamespaceURI)))
                &&
                (ReferenceEquals(_XmlElementNodeName, xmlReader.NameTable.Get(xmlReader.LocalName))))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether this PathNode shall be active.
        /// If it is not active, all match operations return true.
        /// </summary>
        public bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; }
        }

        #endregion
    }

    /// <summary>
    /// This class represents a collection of path nodes that can be used alternatively.
    /// </summary>
    internal class AlternativePathNodesCollection : IPathNode
    {
        #region Fields

        private readonly List<IPathNode> _PathNodes = new List<IPathNode>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternativePathNodesCollection"/> class.
        /// </summary>
        /// <param name="pathNodes">A collection of instances implementing <see cref="IPathNode"/></param>
        internal AlternativePathNodesCollection(params IPathNode[] pathNodes)
        {
            if (pathNodes == null) throw new ArgumentNullException();
            _PathNodes.AddRange(pathNodes);
        }

        #endregion

        #region IPathNode Member

        /// <summary>
        /// This method evaluates all inherent instances of <see cref="IPathNode"/>.
        /// </summary>
        /// <param name="reader">An XmlReader instance</param>
        public bool Matches(XmlReader reader)
        {
            foreach (IPathNode pathNode in _PathNodes)
                if (pathNode.Matches(reader)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether the inherent PathNodes shall be active.
        /// If a PathNode is not active, all match operations return true.
        /// </summary>
        public bool IsActive
        {
            get { return _PathNodes[0].IsActive; }
            set
            {
                foreach (IPathNode pathNode in _PathNodes)
                    pathNode.IsActive = value;
            }
        }

        #endregion
    }
}