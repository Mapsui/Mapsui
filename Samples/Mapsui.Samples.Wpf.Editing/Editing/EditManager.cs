﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.UI;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
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

        readonly DragInfo _dragInfo = new DragInfo();
        readonly AddInfo _addInfo = new AddInfo();
        readonly RotateInfo _rotateInfo = new RotateInfo();
        readonly ScaleInfo _scaleInfo = new ScaleInfo();

        public EditMode EditMode { get; set; }

        public int VertexRadius { get; set; } = 12;

        public bool EndEdit()
        {
            if (_addInfo.Feature == null) return false;

            if (EditMode == EditMode.DrawingLine)
            {
                _addInfo.Vertices?.RemoveAt(_addInfo.Vertices.Count - 1); // correct for double click
                _addInfo.Feature = null;
                _addInfo.Vertex = null;
                EditMode = EditMode.AddLine;
            }
            if (EditMode == EditMode.DrawingPolygon)
            {
                _addInfo.Vertices?.RemoveAt(_addInfo.Vertices.Count - 1); // correct for double click
                var polygon = _addInfo.Feature?.Geometry as Polygon;
                if (polygon == null) return false;
                polygon.ExteriorRing?.Vertices.Add(polygon.ExteriorRing.Vertices.First());
                _addInfo.Feature?.RenderedGeometry?.Clear(); // You need to clear the cache to see changes.
                _addInfo.Feature = null;
                _addInfo.Vertex = null;
                EditMode = EditMode.AddPolygon;
                Layer?.DataHasChanged();
            }

            return false;
        }

        internal void HoveringVertex(MapInfo? mapInfo)
        {
            if (_addInfo.Vertex != null)
            {
                SetPointXY(_addInfo.Vertex, mapInfo?.WorldPosition?.ToPoint());
                _addInfo.Feature?.RenderedGeometry?.Clear();
                Layer?.DataHasChanged();
            }
        }

        public bool AddVertex(Point worldPosition)
        {
            if (EditMode == EditMode.AddPoint)
            {
#pragma warning disable IDISP004 // Don't ignore created IDisposable
                Layer?.Add(new GeometryFeature { Geometry = worldPosition });
#pragma warning restore IDISP004 // Don't ignore created IDisposable
            }
            else if (EditMode == EditMode.AddLine)
            {
                var firstPoint = worldPosition.Clone();
                // Add a second point right away. The second one will be the 'hover' vertex
                var secondPoint = worldPosition.Clone();
                _addInfo.Vertex = secondPoint;
                _addInfo.Feature = new GeometryFeature { Geometry = new LineString(new[] { firstPoint, secondPoint }) };
                _addInfo.Vertices = _addInfo.Feature.Geometry.MainVertices();
                Layer?.Add(_addInfo.Feature);
                Layer?.DataHasChanged();
                EditMode = EditMode.DrawingLine;
            }
            else if (EditMode == EditMode.DrawingLine)
            {
                var lineString = _addInfo.Feature?.Geometry as LineString;
                // Set the final position of the 'hover' vertex (that was already part of the geometry)
                SetPointXY(_addInfo.Vertex, worldPosition.Clone());
                _addInfo.Vertex = worldPosition.Clone(); // and create a new hover vertex
                lineString?.Vertices.Add(_addInfo.Vertex); // and add it to the geometry
                _addInfo.Feature?.RenderedGeometry?.Clear();
                Layer?.DataHasChanged();
            }
            else if (EditMode == EditMode.AddPolygon)
            {
                var firstPoint = worldPosition.Clone();
                // Add a second point right away. The second one will be the 'hover' vertex
                var secondPoint = worldPosition.Clone();
                _addInfo.Vertex = secondPoint;
                _addInfo.Feature = new GeometryFeature
                {
                    Geometry = new Polygon
                    {
                        ExteriorRing = new LinearRing(new[] { firstPoint, secondPoint })
                    }
                };
                _addInfo.Vertices = _addInfo.Feature.Geometry.MainVertices();
                Layer?.Add(_addInfo.Feature);
                Layer?.DataHasChanged();
                EditMode = EditMode.DrawingPolygon;
            }
            else if (EditMode == EditMode.DrawingPolygon)
            {
                var polygon = _addInfo.Feature?.Geometry as Polygon;
                // Set the final position of the 'hover' vertex (that was already part of the geometry)
                SetPointXY(_addInfo.Vertex, worldPosition.Clone());
                _addInfo.Vertex = worldPosition.Clone(); // and create a new hover vertex
                polygon?.ExteriorRing?.Vertices.Add(_addInfo.Vertex); // and add it to the geometry
                _addInfo.Feature?.RenderedGeometry?.Clear();
                Layer?.DataHasChanged();
            }
            return false;
        }

        private static Point? FindVertexTouched(MapInfo mapInfo, IEnumerable<Point> vertices, double screenDistance)
        {
            if (mapInfo.WorldPosition == null)
                return null;

            return vertices.OrderBy(v => v.Distance(mapInfo.WorldPosition.ToPoint()))
                .FirstOrDefault(v => v.Distance(mapInfo.WorldPosition.ToPoint()) < mapInfo.Resolution * screenDistance);
        }

        private void SetPointXY(Point? target, Point? position)
        {
            if (target != null && position != null)
            {
                target.X = position.X;
                target.Y = position.Y;
            }
        }

        public bool StartDragging(MapInfo mapInfo, double screenDistance)
        {
            if (EditMode == EditMode.Modify)
            {
                if (mapInfo.Feature != null)
                {
                    if (mapInfo.Feature is GeometryFeature geometryFeature)
                    {
                        var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainVertices() ?? new List<Point>(), screenDistance);
                        if (vertexTouched != null)
                        {
                            _dragInfo.Feature = geometryFeature;
                            _dragInfo.Vertex = vertexTouched;
                            if (mapInfo.WorldPosition != null && _dragInfo.Vertex != null)
                            {
                                _dragInfo.StartOffsetToVertex = mapInfo.WorldPosition.ToPoint() - _dragInfo.Vertex;
                            }

                            return true; // to indicate start of drag
                        }
                    }
                }
            }
            return false;
        }

        public bool Dragging(Point? worldPosition)
        {
            if (EditMode != EditMode.Modify || _dragInfo.Feature == null || worldPosition == null || _dragInfo.StartOffsetToVertex == null) return false;

            SetPointXY(_dragInfo.Vertex, worldPosition - _dragInfo.StartOffsetToVertex);

            if (_dragInfo.Feature.Geometry is Polygon polygon) // Not this only works correctly it the feature is in the outerring.
            {
                var count = polygon.ExteriorRing?.Vertices.Count ?? 0;
                var vertices = polygon.ExteriorRing?.Vertices ?? new List<Point>();
                var index = vertices.IndexOf(_dragInfo.Vertex!);
                if (index >= 0)
                    // It is a ring where the first should be the same as the last.
                    // So if the first was removed than set the last to the value of the new first
                    if (index == 0) SetPointXY(vertices[count - 1], vertices[0]);
                    // If the last was removed then set the first to the value of the new last
                    else if (index == vertices.Count) SetPointXY(vertices[0], vertices[count - 1]);
            }

            _dragInfo.Feature.RenderedGeometry.Clear();
            Layer?.DataHasChanged();
            return true;
        }

        public void StopDragging()
        {
            if (EditMode == EditMode.Modify && _dragInfo.Feature != null)
            {
                _dragInfo.Feature = null;
            }
        }

        public bool TryDeleteVertex(MapInfo? mapInfo, double screenDistance)
        {
            if (mapInfo?.Feature is GeometryFeature geometryFeature)
            {
                var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainVertices() ?? new List<Point>(), screenDistance);
                if (vertexTouched != null)
                {
                    var vertices = geometryFeature.Geometry?.MainVertices() ?? new List<Point>();
                    var index = vertices.IndexOf(vertexTouched);
                    if (index >= 0)
                    {
                        vertices.RemoveAt(index);
                        var count = vertices.Count;

                        // It is a ring where the first should be the same as the last.
                        // So if the first was removed than set the last to the value of the new first
                        if (index == 0) SetPointXY(vertices[count - 1], vertices[0]);
                        // If the last was removed then set the first to the value of the new last
                        else if (index == vertices.Count) SetPointXY(vertices[0], vertices[count - 1]);

                        geometryFeature.RenderedGeometry.Clear();
                        Layer?.DataHasChanged();
                    }

                }
            }

            return false;
        }

        public bool TryInsertVertex(MapInfo? mapInfo)
        {
            if (mapInfo?.Feature is GeometryFeature geometryFeature)
            {
                var vertices = geometryFeature.Geometry?.MainVertices() ?? new List<Point>();

                if (EditHelper.TryInsertVertex(mapInfo, vertices, VertexRadius))
                {
                    geometryFeature.RenderedGeometry.Clear();
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
                _rotateInfo.Center = geometryFeature.Geometry?.BoundingBox?.Centroid;
            }
            return true; // to signal pan lock
        }

        public bool Rotating(Point? worldPosition)
        {
            if (EditMode != EditMode.Rotate || _rotateInfo.Feature == null || worldPosition == null || _rotateInfo.Center == null || _rotateInfo.PreviousPosition == null) return false;

            var previousVector = _rotateInfo.Center - _rotateInfo.PreviousPosition;
            var currentVector = _rotateInfo.Center - worldPosition;
            var degrees = AngleBetween(currentVector, previousVector);

            if (_rotateInfo.Feature.Geometry != null)
                Geomorpher.Rotate(_rotateInfo.Feature.Geometry, degrees, _rotateInfo.Center);

            _rotateInfo.PreviousPosition = worldPosition;

            _rotateInfo.Feature.RenderedGeometry.Clear();
            Layer?.DataHasChanged();

            return true; // to signal pan lock
        }

        public void StopRotating()
        {
            if (EditMode == EditMode.Rotate && _rotateInfo.Feature != null)
            {
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
                _scaleInfo.Center = geometryFeature.Geometry?.BoundingBox?.Centroid;
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

            _scaleInfo.Feature.RenderedGeometry.Clear();
            Layer?.DataHasChanged();

            return true; // to signal pan lock
        }

        public void StopScaling()
        {
            if (EditMode == EditMode.Scale && _scaleInfo.Feature != null)
            {
                _scaleInfo.Feature = null;
            }
        }

    }
}
