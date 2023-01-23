// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Mapsui.Nts.Providers.Shapefile.Indexing;

/// <summary>
/// Heuristics used for tree generation
/// </summary>
public struct Heuristic
{
    /// <summary>
    /// Maximum tree depth
    /// </summary>
    public int Maxdepth;

    /// <summary>
    /// Minimum Error metric – the volume of a box + a unit cube.
    /// The unit cube in the metric prevents big boxes that happen to be flat having a zero result and muddling things up.
    /// </summary>
    public int Minerror;

    /// <summary>
    /// Minimum object count at node
    /// </summary>
    public int Mintricnt;

    /// <summary>
    /// Target object count at node
    /// </summary>
    public int Tartricnt;
}

/// <summary>
/// Constructs a Quad-tree node from a object list and creates its children recursively
/// </summary>
public class QuadTree : IDisposable
{
    private MRect? _box;
    private QuadTree? _child0;
    private QuadTree? _child1;

    /// <summary>
    /// Nodes depth in a tree
    /// </summary>
    private uint _depth;

    private List<BoxObjects>? _objList;

    /// <summary>
    /// Creates a node and either splits the objects recursively into sub-nodes, or stores them at the node depending on the heuristics.
    /// Tree is built top->down
    /// </summary>
    /// <param name="objList">Geometries to index</param>
    /// <param name="depth">Current depth of tree</param>
    /// <param name="heurdata">Heuristics data</param>
    public QuadTree(List<BoxObjects> objList, uint depth, Heuristic heurdata)
    {
        _depth = depth;

        _box = objList[0].Box;
        for (var i = 0; i < objList.Count; i++)
            _box = _box.Join(objList[i].Box);

        // test our build heuristic - if passes, make children
        if (depth < heurdata.Maxdepth && objList.Count > heurdata.Mintricnt &&
            (objList.Count > heurdata.Tartricnt || ErrorMetric(_box) > heurdata.Minerror))
        {
            var objBuckets = new List<BoxObjects>[2]; // buckets of geometries
            objBuckets[0] = new List<BoxObjects>();
            objBuckets[1] = new List<BoxObjects>();

            var useXAxis = _box.Width > _box.Height; // longest axis
            double geoAverage = 0; // geometric average - midpoint of ALL the objects

            // go through all bbox and calculate the average of the midpoints
            double fraction = 1.0f / objList.Count;
            for (var i = 0; i < objList.Count; i++)
            {
                var centroid = useXAxis ? objList[i].Box.Centroid.X : objList[i].Box.Centroid.Y;
                geoAverage += centroid * fraction;
            }

            // bucket bbox based on their midpoint's side of the geo average in the longest axis
            for (var i = 0; i < objList.Count; i++)
            {
                var centroid = useXAxis ? objList[i].Box.Centroid.X : objList[i].Box.Centroid.Y;
                objBuckets[geoAverage > centroid ? 1 : 0].Add(objList[i]);
            }

            //If objects couldn't be split, just store them at the leaf
            //TODO: Try splitting on another axis
            if (objBuckets[0].Count == 0 || objBuckets[1].Count == 0)
            {
                _child0 = null;
                _child1 = null;
                // copy object list
                _objList = objList;
            }
            else
            {
                // create new children using the buckets
                _child0 = new QuadTree(objBuckets[0], depth + 1, heurdata);
                _child1 = new QuadTree(objBuckets[1], depth + 1, heurdata);
            }
        }
        else
        {
            // otherwise the build heuristic failed, this is 
            // set the first child to null (identifies a leaf)
            _child0 = null;
            _child1 = null;
            // copy object list
            _objList = objList;
        }
    }

    /// <summary>
    /// This constructor is used internally for loading a tree from a file
    /// </summary>
    private QuadTree()
    {
        _box = null;
    }


    private const double Indexfileversion = 1.0;

    /// <summary>
    /// Loads a quadtree from a file
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static QuadTree FromFile(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (br.ReadDouble() != Indexfileversion) //Check fileindex version
        {
            fs.Close();
            throw new ObsoleteFileFormatException(
                "Invalid index file version. Please rebuild the spatial index by either deleting the index");
        }
        var node = ReadNode(0, in br);
        br.Close();
        fs.Close();
        return node;
    }

    /// <summary>
    /// Reads a node from a stream recursively
    /// </summary>
    /// <param name="depth">Current depth</param>
    /// <param name="br">Binary reader reference</param>
    /// <returns></returns>
    private static QuadTree ReadNode(uint depth, in BinaryReader br)
    {
        var node = new QuadTree
        {
            _depth = depth,
            Box = new MRect(br.ReadDouble(), br.ReadDouble(), br.ReadDouble(), br.ReadDouble())
        };
        var isLeaf = br.ReadBoolean();
        if (isLeaf)
        {
            var featureCount = br.ReadInt32();
            node._objList = new List<BoxObjects>();
            for (var i = 0; i < featureCount; i++)
            {
                var box = new BoxObjects
                {
                    Box = new MRect(
                        br.ReadDouble(), br.ReadDouble(),
                        br.ReadDouble(), br.ReadDouble()),
                    Id = (uint)br.ReadInt32()
                };
                node._objList.Add(box);
            }
        }
        else
        {
            node.Child0 = ReadNode(node._depth + 1, in br);
            node.Child1 = ReadNode(node._depth + 1, in br);
        }
        return node;
    }

    /// <summary>
    /// Saves the Quadtree to a file
    /// </summary>
    /// <param name="filename"></param>
    public void SaveIndex(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        bw.Write(Indexfileversion); //Save index version
        SaveNode(this, in bw);
        bw.Close();
        fs.Close();
    }

    /// <summary>
    /// Saves a node to a stream recursively
    /// </summary>
    /// <param name="node">Node to save</param>
    /// <param name="sw">Reference to BinaryWriter</param>
    private void SaveNode(QuadTree? node, in BinaryWriter sw)
    {
        if (node == null || node.Box == null)
            return;
        //Write node BoundingBox
        sw.Write(node.Box.Min.X);
        sw.Write(node.Box.Min.Y);
        sw.Write(node.Box.Max.X);
        sw.Write(node.Box.Max.Y);
        sw.Write(node.IsLeaf);
        if (node.IsLeaf && node._objList != null)
        {
            sw.Write(node._objList.Count); //Write number of features at node
            for (var i = 0; i < node._objList.Count; i++) //Write each feature box
            {
                sw.Write(node._objList[i].Box.Min.X);
                sw.Write(node._objList[i].Box.Min.Y);
                sw.Write(node._objList[i].Box.Max.X);
                sw.Write(node._objList[i].Box.Max.Y);
                sw.Write(node._objList[i].Id);
            }
        }
        else if (!node.IsLeaf) //Save next node
        {
            SaveNode(node.Child0, in sw);
            SaveNode(node.Child1, in sw);
        }
    }

    public class ObsoleteFileFormatException : Exception
    {
        /// <summary>
        /// Exception thrown when layer rendering has failed
        /// </summary>
        /// <param name="message"></param>
        public ObsoleteFileFormatException(string message)
            : base(message)
        {
        }
    }


    /// <summary>
    /// Determines whether the node is a leaf (if data is stored at the node, we assume the node is a leaf)
    /// </summary>
    public bool IsLeaf => _objList != null;

    /// <summary>
    /// Gets/sets the Axis Aligned Bounding Box
    /// </summary>
    public MRect? Box
    {
        get => _box;
        set => _box = value;
    }

    /// <summary>
    /// Gets/sets the left child node
    /// </summary>
    public QuadTree? Child0
    {
        get => _child0;
        set => _child0 = value;
    }

    /// <summary>
    /// Gets/sets the right child node
    /// </summary>
    public QuadTree? Child1
    {
        get => _child1;
        set => _child1 = value;
    }

    /// <summary>
    /// Gets the depth of the current node in the tree
    /// </summary>
    public uint Depth => _depth;

    /// <summary>
    /// Disposes the node
    /// </summary>
    public virtual void Dispose()
    {
        //this._box = null;
        _child0?.Dispose();
        _child0 = null;
        _child1?.Dispose();
        _child1 = null;
        _objList = null;
    }


    /// <summary>
    /// Calculate the floating point error metric 
    /// </summary>
    /// <returns></returns>
    public double ErrorMetric(MRect box)
    {
        var temp = new MPoint(1, 1) + (box.Max - box.Min);
        return temp.X * temp.Y;
    }

    /// <summary>
    /// Searches the tree and looks for intersections with the BoundingBox 'bbox'
    /// </summary>
    /// <param name="box">BoundingBox to intersect with</param>
    public Collection<uint> Search(MRect box)
    {
        var objectlist = new Collection<uint>();
        IntersectTreeRecursive(box, this, in objectlist);
        return objectlist;
    }

    /// <summary>
    /// Recursive function that traverses the tree and looks for intersections with a BoundingBox
    /// </summary>
    /// <param name="box">BoundingBox to intersect with</param>
    /// <param name="node">Node to search from</param>
    /// <param name="list">List of found intersections</param>
    private void IntersectTreeRecursive(MRect box, QuadTree? node, in Collection<uint> list)
    {
        if (node == null)
            return;
        if (node.IsLeaf) //Leaf has been reached
        {
            if (node._objList != null)
            {
                foreach (var boxObject in node._objList)
                {
                    if (box.Intersects(boxObject.Box))
                        list.Add(boxObject.Id);

                }
            }
        }
        else
        {
            if (node.Box?.Intersects(box) ?? false)
            {
                IntersectTreeRecursive(box, node.Child0, in list);
                IntersectTreeRecursive(box, node.Child1, in list);
            }
        }
    }


    /// <summary>
    /// BoundingBox and Feature ID structure used for storing in the quadtree 
    /// </summary>
    public struct BoxObjects
    {
        /// <summary>
        /// BoundingBox
        /// </summary>
        public MRect Box;

        /// <summary>
        /// Feature ID
        /// </summary>
        public uint Id;
    }
}
