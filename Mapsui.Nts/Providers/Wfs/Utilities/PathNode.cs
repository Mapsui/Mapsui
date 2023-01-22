// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System;
using System.Collections.Generic;
using System.Xml;

namespace Mapsui.Providers.Wfs.Utilities;

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

    private readonly string _xmlElementNodeName;
    private readonly string _xmlElementNsUri;
    private bool _isActive = true;

    /// <summary>
    /// Gets the namespace URI of the element-node
    /// </summary>
    internal string XmlElementNsUri => _xmlElementNsUri;

    /// <summary>
    /// Gets the local name of the element-node
    /// </summary>
    internal string XmlElementNodeName => _xmlElementNodeName;



    /// <summary>
    /// Initializes a new instance of the <see cref="PathNode"/> class.
    /// </summary>
    /// <param name="elementNsUri">The namespace URI of the element-node</param>
    /// <param name="elementNodeName">The local name of the element-node</param>
    /// <param name="nameTable">A NameTable for storing namespace URI and local name</param>
    internal PathNode(string elementNsUri, string elementNodeName, NameTable nameTable)
    {
        _xmlElementNsUri = nameTable.Add(elementNsUri);
        _xmlElementNodeName = nameTable.Add(elementNodeName);
    }



    /// <summary>
    /// This method evaluates, if the position of an XmlReader is at the element-node represented by the instance of this class.
    /// It compares pointers instead of literal values due to performance reasons.
    /// Therefore the name table of the XmlReader given as argument must be the one handed over to the constructor.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    public bool Matches(XmlReader xmlReader)
    {
        if (!_isActive) return true;

        //Compare pointers instead of literal values
        if (xmlReader.NameTable != null &&
            ((ReferenceEquals(_xmlElementNsUri, xmlReader.NameTable.Get(xmlReader.NamespaceURI))) &&
             (ReferenceEquals(_xmlElementNodeName, xmlReader.NameTable.Get(xmlReader.LocalName)))))
            return true;

        return false;
    }

    /// <summary>
    /// Determines whether this PathNode shall be active.
    /// If it is not active, all match operations return true.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

}

/// <summary>
/// This class represents a collection of path nodes that can be used alternatively.
/// </summary>
internal class AlternativePathNodesCollection : IPathNode
{

    private readonly List<IPathNode> _pathNodes = new List<IPathNode>();



    /// <summary>
    /// Initializes a new instance of the <see cref="AlternativePathNodesCollection"/> class.
    /// </summary>
    /// <param name="pathNodes">A collection of instances implementing <see cref="IPathNode"/></param>
    internal AlternativePathNodesCollection(params IPathNode[] pathNodes)
    {
        if (pathNodes == null) throw new ArgumentNullException();
        _pathNodes.AddRange(pathNodes);
    }



    /// <summary>
    /// This method evaluates all inherent instances of <see cref="IPathNode"/>.
    /// </summary>
    /// <param name="reader">An XmlReader instance</param>
    public bool Matches(XmlReader reader)
    {
        foreach (var pathNode in _pathNodes)
            if (pathNode.Matches(reader)) return true;
        return false;
    }

    /// <summary>
    /// Determines whether the inherent PathNodes shall be active.
    /// If a PathNode is not active, all match operations return true.
    /// </summary>
    public bool IsActive
    {
        get => _pathNodes[0].IsActive;
        set
        {
            foreach (var pathNode in _pathNodes)
                pathNode.IsActive = value;
        }
    }

}
