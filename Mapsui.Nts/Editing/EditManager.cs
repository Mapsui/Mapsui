using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Editing;

public enum EditMode
{
    None,
    AddPoint,
    AddLine,
    DrawingLine,
    AddPolygon,
    DrawingPolygon,
    Modify,
    Rotate,
    Scale
}

public class EditManager
{
    public WritableLayer? Layer { get; set; }

    private readonly DragInfo _dragInfo = new();
    private readonly AddInfo _addInfo = new();
    private readonly RotateInfo _rotateInfo = new();
    private readonly ScaleInfo _scaleInfo = new();

    public EditMode EditMode { get; set; }

    public int VertexRadius { get; set; } = 12;
    public bool SelectMode { get; set; }

    /// <summary>
    /// Invoked after a new vertex is added.
    /// Derived classes can override this method, to perform custom actions on the newly-added vertex.
    /// </summary>
    /// <param name="geometryFeature">The geometry feature.</param>
    /// <param name="geometry">The geometry.</param>
    /// <param name="newVertex">The newly-added vertex.</param>
    protected virtual void VertexAdded(GeometryFeature geometryFeature, Geometry geometry, Coordinate newVertex)
    {
    }

    /// <summary>
    /// Invoked after a polygon is drawn.
    /// Derived classes can override this method, to perform custom actions on the newly-drawn polygon.
    /// </summary>
    /// <param name="geometryFeature">The geometry feature.</param>
    /// <param name="polygon">The newly-drawn polygon.</param>
    protected virtual void PolygonDrawn(GeometryFeature geometryFeature, Polygon polygon)
    {
    }

    /// <summary>
    /// Invoked after a coordinate is deleted.
    /// Derived classes can override this method, to perform custom actions, according to the deleted coordinate.
    /// </summary>
    /// <param name="geometryFeature">The geometry feature.</param>
    /// <param name="oldGeometry">The old geometry.</param>
    /// <param name="deletedCoordinate">The deleted coordinate.</param>
    /// <param name="index">The index.</param>
    protected virtual void CoordinateDeleted(GeometryFeature geometryFeature, Geometry? oldGeometry, Coordinate deletedCoordinate, int index)
    {
    }

    /// <summary>
    /// Invoked after a new coordinate is inserted.
    /// Derived classes can override this method, to perform custom actions, according to the deleted coordinate.
    /// </summary>
    /// <param name="geometryFeature">The geometry feature.</param>
    /// <param name="newGeometry">The geometry, after the coordinate has been inserted.</param>
    /// <param name="insertedCoordinate">The inserted coordinate.</param>
    protected virtual void CoordinateInserted(GeometryFeature geometryFeature, Geometry? newGeometry, Coordinate insertedCoordinate)
    {
    }

    public bool EndEdit()
    {
        if (_addInfo.Feature is null) return false;
        if (_addInfo.Vertices is null) return false;

        if (EditMode == EditMode.DrawingLine)
        {
            // line needs to be at least 2 vertices
            if (_addInfo.Vertices.Count > 2)
                _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // Remove duplicate last element added by the final double click

            _addInfo.Feature.Geometry = new LineString(_addInfo.Vertices.ToArray());

            _addInfo.Feature = null;
            _addInfo.Vertex = null;
            EditMode = EditMode.AddLine;
        }
        else if (EditMode == EditMode.DrawingPolygon)
        {
            // Polygon needs to be at least 3 vertices
            if (_addInfo.Vertices.Count > 3)
                _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // correct for double click
            var polygon = _addInfo.Feature.Geometry as Polygon;
            if (polygon == null) return false;

            var linearRing = _addInfo.Vertices.ToList();
            linearRing.Add(linearRing[0].Copy()); // Add first coordinate at end to close the ring.
            var geometry = new Polygon(new LinearRing(linearRing.ToArray()));
            var feature = _addInfo.Feature;
            _addInfo.Feature.Geometry = geometry;

            _addInfo.Feature.Modified(); // You need to clear the cache to see changes.
            _addInfo.Feature = null;
            _addInfo.Vertex = null;
            EditMode = EditMode.AddPolygon;
            PolygonDrawn(feature, geometry);
            Layer?.DataHasChanged();
        }

        return false;
    }

    public void HoveringVertex(MapInfo? mapInfo)
    {
        if (_addInfo.Vertex != null)
        {
            _addInfo.Vertex.SetXY(mapInfo?.WorldPosition);
            _addInfo.Feature?.Modified();
            Layer?.DataHasChanged();
        }
    }

    public bool AddVertex(Coordinate worldPosition)
    {
        if (EditMode == EditMode.AddPoint)
        {
            var geometry = worldPosition.ToMPoint().ToPoint();
            var newGeometryFeature = new GeometryFeature { Geometry = geometry };
            Layer?.Add(newGeometryFeature);
            VertexAdded(newGeometryFeature, geometry, worldPosition);
            Layer?.DataHasChanged();
        }
        else if (EditMode == EditMode.AddLine)
        {
            var firstPoint = worldPosition.Copy();
            // Add a second point right away. The second one will be the 'hover' vertex
            var secondPoint = worldPosition.Copy();
            _addInfo.Vertex = secondPoint;
            var geometry = new LineString(new[] { firstPoint, secondPoint });
            var newGeometryFeature = new GeometryFeature { Geometry = geometry };
            _addInfo.Feature = newGeometryFeature;
            _addInfo.Vertices = _addInfo.Feature.Geometry.MainCoordinates();
            Layer?.Add(_addInfo.Feature);
            VertexAdded(newGeometryFeature, geometry, worldPosition);
            Layer?.DataHasChanged();
            EditMode = EditMode.DrawingLine;
        }
        else if (EditMode == EditMode.DrawingLine)
        {
            if (_addInfo.Feature is null) return false;
            if (_addInfo.Vertices is null) return false;

            // Set the final position of the 'hover' vertex (that was already part of the geometry)
            _addInfo.Vertex.SetXY(worldPosition);
            _addInfo.Vertex = worldPosition.Copy(); // and create a new hover vertex
            _addInfo.Vertices.Add(_addInfo.Vertex);
            var geometry = new LineString(_addInfo.Vertices.ToArray());
            _addInfo.Feature.Geometry = geometry;
            _addInfo.Feature?.Modified();
            VertexAdded(_addInfo.Feature!, geometry, worldPosition);
            Layer?.DataHasChanged();
        }
        else if (EditMode == EditMode.AddPolygon)
        {
            var firstPoint = worldPosition.Copy();
            // Add a second point right away. The second one will be the 'hover' vertex
            var secondPoint = worldPosition.Copy();
            _addInfo.Vertex = secondPoint;
            _addInfo.Vertices = new List<Coordinate>(new[] { firstPoint, secondPoint });

            var geometry = new Polygon(new LinearRing(new[] { firstPoint, secondPoint, firstPoint })); // A LinearRing needs at least three coordinates
            var feature = new GeometryFeature()
            {
                Geometry = geometry
            };
            _addInfo.Feature = feature;
            Layer?.Add(_addInfo.Feature);
            VertexAdded(feature, geometry, worldPosition);
            Layer?.DataHasChanged();
            EditMode = EditMode.DrawingPolygon;
        }
        else if (EditMode == EditMode.DrawingPolygon)
        {
            if (_addInfo.Feature is null) return false;
            if (_addInfo.Vertices is null) return false;

            // Set the final position of the 'hover' vertex (that was already part of the geometry)
            _addInfo.Vertex.SetXY(worldPosition);
            _addInfo.Vertex = worldPosition.Copy(); // and create a new hover vertex
            _addInfo.Vertices.Add(_addInfo.Vertex);

            var linearRing = _addInfo.Vertices.ToList();
            linearRing.Add(linearRing[0]); // Add first coordinate at end to close the ring.
            var geometry = new Polygon(new LinearRing(linearRing.ToArray()));
            _addInfo.Feature.Geometry = geometry;

            _addInfo.Feature?.Modified();
            VertexAdded(_addInfo.Feature!, geometry, worldPosition);
            Layer?.DataHasChanged();
        }
        return false;
    }

    private static Coordinate? FindVertexTouched(MapInfo mapInfo, IEnumerable<Coordinate> vertices, double screenDistance)
    {
        if (mapInfo.WorldPosition == null)
            return null;

        return vertices.OrderBy(v => v.Distance(mapInfo.WorldPosition.ToCoordinate()))
            .FirstOrDefault(v => v.Distance(mapInfo.WorldPosition.ToCoordinate()) < mapInfo.Resolution * screenDistance);
    }

    public bool StartDragging(MapInfo mapInfo, double screenDistance)
    {
        if (EditMode == EditMode.Modify)
        {
            if (mapInfo.Feature != null)
            {
                if (mapInfo.Feature is GeometryFeature geometryFeature)
                {
                    var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainCoordinates() ?? new List<Coordinate>(), screenDistance);
                    _dragInfo.Feature = geometryFeature;
                    _dragInfo.Vertex = vertexTouched;
                    if (mapInfo.WorldPosition != null)
                    {
                        if (_dragInfo.Vertex != null)
                        {
                            _dragInfo.DraggingFeature = false;
                            _dragInfo.StartOffsetToVertex = mapInfo.WorldPosition - _dragInfo.Vertex.ToMPoint();
                        }
                        else if (_dragInfo.Feature != null && mapInfo.Feature.Extent != null)
                        {
                            _dragInfo.StartOffsetToVertex = mapInfo.WorldPosition - mapInfo.Feature.Extent.Centroid;
                            _dragInfo.Vertex = mapInfo.Feature.Extent.Centroid.ToCoordinate();
                            _dragInfo.DraggingFeature = true;
                        }
                    }

                    return true; // to indicate start of drag
                }
            }
        }
        return false;
    }

    public bool Dragging(Point? worldPosition)
    {
        if (EditMode != EditMode.Modify || _dragInfo.Feature == null || worldPosition == null || (_dragInfo.StartOffsetToVertex == null)) return false;

        if (_dragInfo.Vertex != null)
        {
            // only modify the vertex if it is not moving a feature
            if (!_dragInfo.DraggingFeature)
            {
                _dragInfo.Vertex.SetXY(worldPosition.ToMPoint() - _dragInfo.StartOffsetToVertex);

                if (_dragInfo.Feature
                        .Geometry is Polygon
                    polygon) // Not this only works correctly it the feature is in the outer ring.
                {
                    var count = polygon.ExteriorRing?.Coordinates.Length ?? 0;
                    var vertices = polygon.ExteriorRing?.Coordinates ?? Array.Empty<Coordinate>();
                    var index = vertices.ToList().IndexOf(_dragInfo.Vertex!);
                    if (index >= 0)
                        // It is a ring where the first should be the same as the last.
                        // So if the first was removed than set the last to the value of the new first
                        if (index == 0) vertices[count - 1].SetXY(vertices[0]);
                        // If the last was removed then set the first to the value of the new last
                        else if (index == vertices.Length) vertices[0].SetXY(vertices[count - 1]);
                }
            }
            else // NEW: try to drag the whole feature when the position of dragging is inside the geometry
            {
                MPoint previousVertex = _dragInfo.Vertex.ToMPoint(); // record the previous position
                MPoint newVertex = worldPosition.ToMPoint() - _dragInfo.StartOffsetToVertex; // new position

                if (_dragInfo.Feature.Geometry is Polygon polygon)
                {
                    var vertices = polygon.ExteriorRing?.Coordinates ?? Array.Empty<Coordinate>();
                    foreach (Coordinate vtx in vertices) // modify every vertex on the ring
                    {
                        vtx.SetXY(vtx.ToMPoint() + (newVertex - previousVertex)); // adding the offset
                    }
                }
                else if (_dragInfo.Feature.Geometry is LineString lineString)
                {
                    var vertices = lineString.Coordinates ?? Array.Empty<Coordinate>();
                    foreach (Coordinate vtx in vertices) // modify every vertex on the line
                    {
                        vtx.SetXY(vtx.ToMPoint() + (newVertex - previousVertex)); // adding the offset
                    }
                }
                else if (_dragInfo.Feature.Geometry is Point point)
                {
                    var vertex = point.Coordinate;
                    vertex.SetXY(vertex.ToMPoint() + (newVertex - previousVertex)); // adding the offset
                }

                _dragInfo.Vertex.SetXY(worldPosition.ToMPoint() - _dragInfo.StartOffsetToVertex);
            }
        }

        _dragInfo.Feature.Modified();
        Layer?.DataHasChanged();
        return true;
    }

    public void StopDragging()
    {
        if (EditMode == EditMode.Modify && _dragInfo.Feature != null)
        {
            _dragInfo.Feature.Geometry?.GeometryChanged();
            _dragInfo.Feature = null;
        }
    }

    public bool TryDeleteCoordinate(MapInfo? mapInfo, double screenDistance)
    {
        if (mapInfo?.Feature is GeometryFeature geometryFeature)
        {
            var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainCoordinates() ?? new List<Coordinate>(), screenDistance);
            if (vertexTouched != null)
            {
                var vertices = geometryFeature.Geometry?.MainCoordinates() ?? new List<Coordinate>();
                var index = vertices.IndexOf(vertexTouched);
                if (index >= 0)
                {
                    var oldGeometry = geometryFeature.Geometry;
                    geometryFeature.Geometry = oldGeometry.DeleteCoordinate(index);
                    geometryFeature.Modified();
                    CoordinateDeleted(geometryFeature, oldGeometry, vertexTouched, index);
                    Layer?.DataHasChanged();
                }
            }
        }

        return false;
    }

    public bool TryInsertCoordinate(MapInfo? mapInfo)
    {
        if (mapInfo?.WorldPosition is null) return false;

        if (mapInfo.Feature is GeometryFeature geometryFeature)
        {
            if (geometryFeature.Geometry is null) return false;

            var vertices = geometryFeature.Geometry.MainCoordinates();
            if (EditHelper.ShouldInsert(mapInfo.WorldPosition, mapInfo.Resolution, vertices, VertexRadius, out var segment))
            {
                var coordinate = mapInfo.WorldPosition.ToCoordinate();
                geometryFeature.Geometry = geometryFeature.Geometry.InsertCoordinate(coordinate, segment);
                geometryFeature.Modified();
                CoordinateInserted(geometryFeature, geometryFeature.Geometry, coordinate);
                Layer?.DataHasChanged();
            }
        }
        return false;
    }

    public bool StartRotating(MapInfo mapInfo)
    {
        if (mapInfo.Feature is GeometryFeature geometryFeature)
        {
            if (EditMode != EditMode.Rotate) return false;

            _rotateInfo.Feature = geometryFeature;
            _rotateInfo.PreviousPosition = mapInfo.WorldPosition.ToPoint();
            _rotateInfo.Center = geometryFeature.Geometry?.Centroid;
        }
        return true; // to signal pan lock
    }

    public bool Rotating(Point? worldPosition)
    {
        if (EditMode != EditMode.Rotate || _rotateInfo.Feature == null || worldPosition == null || _rotateInfo.Center == null || _rotateInfo.PreviousPosition == null) return false;

        var previousVector = new Point(
            _rotateInfo.Center.X - _rotateInfo.PreviousPosition.X,
            _rotateInfo.Center.Y - _rotateInfo.PreviousPosition.Y);
        var currentVector = new Point(
            _rotateInfo.Center.X - worldPosition.X,
            _rotateInfo.Center.Y - worldPosition.Y);
        var degrees = AngleBetween(currentVector, previousVector);

        if (_rotateInfo.Feature.Geometry != null)
            Geomorpher.Rotate(_rotateInfo.Feature.Geometry, degrees, _rotateInfo.Center);

        _rotateInfo.PreviousPosition = worldPosition;

        _rotateInfo.Feature.Modified();
        Layer?.DataHasChanged();

        return true; // to signal pan lock
    }

    public void StopRotating()
    {
        if (EditMode == EditMode.Rotate && _rotateInfo.Feature != null)
        {
            _rotateInfo.Feature.Geometry?.GeometryChanged();
            _rotateInfo.Feature = null;
        }
    }

    public static double AngleBetween(Point vector1, Point vector2)
    {
        var sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
        var cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

        return Math.Atan2(sin, cos) * (180 / Math.PI);
    }

    public bool StartScaling(MapInfo mapInfo)
    {
        if (mapInfo.Feature is GeometryFeature geometryFeature)
        {
            if (EditMode != EditMode.Scale) return false;

            _scaleInfo.Feature = geometryFeature;
            _scaleInfo.PreviousPosition = mapInfo.WorldPosition.ToPoint();
            _scaleInfo.Center = geometryFeature.Geometry?.Centroid;
        }

        return true; // to signal pan lock
    }

    public bool Scaling(Point? worldPosition)
    {
        if (EditMode != EditMode.Scale || _scaleInfo.Feature == null || worldPosition == null || _scaleInfo.PreviousPosition == null || _scaleInfo.Center == null) return false;

        var scale =
            _scaleInfo.Center.Distance(worldPosition) /
            _scaleInfo.Center.Distance(_scaleInfo.PreviousPosition);


        if (_scaleInfo.Feature.Geometry != null)
            Geomorpher.Scale(_scaleInfo.Feature.Geometry, scale, _scaleInfo.Center);

        _scaleInfo.PreviousPosition = worldPosition;

        _scaleInfo.Feature.Modified();
        Layer?.DataHasChanged();

        return true; // to signal pan lock
    }

    public void StopScaling()
    {
        if (EditMode == EditMode.Scale && _scaleInfo.Feature != null)
        {
            _scaleInfo.Feature.Geometry?.GeometryChanged();
            _scaleInfo.Feature = null;
        }
    }
}
