using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
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

    // In the current implementation you have to tap within the polygon itself to get a hit,
    // no matter how big the vertex is. This could be resolved by introducing a VertexOnlyStyle
    // to replace the VertexOnlyLayer.
    public int VertexRadius { get; set; } = 12;
    public bool SelectMode { get; set; }

    public bool EndEdit()
    {
        if (_addInfo.Feature is null) return false;
        if (_addInfo.Vertices is null) return false;

        if (EditMode == EditMode.DrawingLine)
        {
            _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // Remove the last vertex, because it is the hover vertex
            _addInfo.Feature.Geometry = new LineString([.. _addInfo.Vertices]);

            _addInfo.Feature = null;
            _addInfo.Vertex = null;
            EditMode = EditMode.AddLine;
        }
        else if (EditMode == EditMode.DrawingPolygon)
        {
            var polygon = _addInfo.Feature.Geometry as Polygon;
            if (polygon == null) return false;

            _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // Remove the last vertex, because it is the hover vertex
            var linearRing = _addInfo.Vertices.ToList();
            linearRing.Add(linearRing[0].Copy()); // Add first coordinate at end to close the ring.
            _addInfo.Feature.Geometry = new Polygon(new LinearRing([.. linearRing]));

            _addInfo.Feature.Modified(); // You need to clear the cache to see changes.
            _addInfo.Feature = null;
            _addInfo.Vertex = null;
            EditMode = EditMode.AddPolygon;
            Layer?.DataHasChanged();
        }

        return false;
    }

    public void HoveringVertex(MapInfo mapInfo)
    {
        if (_addInfo.Vertex != null)
        {
            _addInfo.Vertex.SetXY(mapInfo.WorldPosition);
            _addInfo.Feature?.Modified();
            Layer?.DataHasChanged();
        }
    }

    public bool AddVertex(Coordinate worldPosition)
    {
        if (EditMode == EditMode.AddPoint)
        {
            Layer?.Add(new GeometryFeature { Geometry = worldPosition.ToMPoint().ToPoint() });
            Layer?.DataHasChanged();
        }
        else if (EditMode == EditMode.AddLine)
        {
            var firstPoint = worldPosition.Copy();
            // Add a second point right away. The second one will be the 'hover' vertex
            var secondPoint = worldPosition.Copy();
            _addInfo.Vertex = secondPoint;
            _addInfo.Feature = new GeometryFeature { Geometry = new LineString([firstPoint, secondPoint]) };
            _addInfo.Vertices = _addInfo.Feature.Geometry.MainCoordinates();
            Layer?.Add(_addInfo.Feature);
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
            _addInfo.Feature.Geometry = new LineString([.. _addInfo.Vertices]);

            _addInfo.Feature?.Modified();
            Layer?.DataHasChanged();
        }
        else if (EditMode == EditMode.AddPolygon)
        {
            var firstPoint = worldPosition.Copy();
            // Add a second point right away. The second one will be the 'hover' vertex
            var secondPoint = worldPosition.Copy();
            _addInfo.Vertex = secondPoint;
            _addInfo.Vertices = new List<Coordinate>([firstPoint, secondPoint]);

            _addInfo.Feature = new GeometryFeature(new Polygon(ToLinearRing(_addInfo.Vertices)));

            Layer?.Add(_addInfo.Feature);
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
            _addInfo.Feature.Geometry = new Polygon(ToLinearRing(_addInfo.Vertices));

            _addInfo.Feature?.Modified();
            Layer?.DataHasChanged();
        }
        return false;
    }

    private static LinearRing ToLinearRing(IList<Coordinate> vertices)
    {
        var linearRing = vertices.ToList();
        linearRing.Add(linearRing[0]); // Add first coordinate at end to close the ring.
        return new LinearRing([.. linearRing]);
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
                    var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainCoordinates() ?? [], screenDistance);
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

                if (_dragInfo.Feature.Geometry is Polygon polygon)
                {
                    // Note, this only works correctly if the feature is in the outer ring.
                    var count = polygon.ExteriorRing?.Coordinates.Length ?? 0;
                    var vertices = polygon.ExteriorRing?.Coordinates ?? Array.Empty<Coordinate>();
                    var index = vertices.ToList().IndexOf(_dragInfo.Vertex!);
                    if (index >= 0)
                    {
                        // It is a ring where the first should be the same as the last.
                        // So if the first was modified than set the last to the value of the new first
                        if (index == 0)
                            vertices[count - 1].SetXY(vertices[0]);
                        else if (index == count - 1)
                            vertices[0].SetXY(vertices[count - 1]);
                    }
                }
            }
            else // Try to drag the whole feature when the position of dragging is inside the geometry
            {
                MPoint previousVertex = _dragInfo.Vertex.ToMPoint(); // record the previous position
                MPoint newVertex = worldPosition.ToMPoint() - _dragInfo.StartOffsetToVertex; // new position

                if (_dragInfo.Feature.Geometry is Polygon polygon)
                {
                    var vertices = polygon.ExteriorRing?.Coordinates ?? Array.Empty<Coordinate>();
                    foreach (Coordinate vtx in vertices) // Modify every vertex on the ring
                    {
                        vtx.SetXY(vtx.ToMPoint() + (newVertex - previousVertex)); // Adding the offset
                    }
                }
                else if (_dragInfo.Feature.Geometry is LineString lineString)
                {
                    var vertices = lineString.Coordinates ?? Array.Empty<Coordinate>();
                    foreach (Coordinate vtx in vertices) // modify every vertex on the line
                    {
                        vtx.SetXY(vtx.ToMPoint() + (newVertex - previousVertex)); // Adding the offset
                    }
                }
                else if (_dragInfo.Feature.Geometry is Point point)
                {
                    var vertex = point.Coordinate;
                    vertex.SetXY(vertex.ToMPoint() + (newVertex - previousVertex)); // Adding the offset
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

    public bool TryDeleteCoordinate(MapInfo mapInfo, double screenDistance)
    {
        if (mapInfo.Feature is GeometryFeature geometryFeature)
        {
            var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainCoordinates() ?? [], screenDistance);
            if (vertexTouched != null)
            {
                var vertices = geometryFeature.Geometry?.MainCoordinates() ?? [];
                var index = vertices.IndexOf(vertexTouched);
                if (index >= 0)
                {
                    geometryFeature.Geometry = geometryFeature.Geometry.DeleteCoordinate(index);
                    geometryFeature.Modified();
                    Layer?.DataHasChanged();
                }
            }
        }

        return false;
    }

    public bool TryInsertCoordinate(MapInfo mapInfo)
    {
        if (mapInfo.Feature is GeometryFeature geometryFeature)
        {
            if (geometryFeature.Geometry is null) return false;

            var vertices = geometryFeature.Geometry.MainCoordinates();
            if (EditHelper.ShouldInsert(mapInfo.WorldPosition, mapInfo.Resolution, vertices, VertexRadius, out var segment))
            {
                geometryFeature.Geometry = geometryFeature.Geometry.InsertCoordinate(mapInfo.WorldPosition.ToCoordinate(), segment);
                geometryFeature.Modified();
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
        return true; // To signal pan lock
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

    public void ResetManipulations()
    {
        _dragInfo.Reset();
        _rotateInfo.Reset();
        _scaleInfo.Reset();

        // Do not reset AddInfo, because it shows the next vertex to be added in hover mode.
        // The AddInfo should be reset when the geometry is closed.
        // _addInfo.Reset();
    }

    public bool IsManipulating()
    {
        return _dragInfo.Feature != null || _rotateInfo.Feature != null || _scaleInfo.Feature != null;
    }

    public MRect? GetGrownExtent()
    {
        if (Layer?.Extent is null)
            return null;

        return Layer.Extent.Grow(Layer.Extent.Width * 0.2);
    }
}
