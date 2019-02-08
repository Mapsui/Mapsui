using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
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
        public WritableLayer Layer { get; set; }

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
                _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // correct for double click
                _addInfo.Feature = null;
                _addInfo.Vertex = null;
                EditMode = EditMode.AddLine;
            }
            if (EditMode == EditMode.DrawingPolygon)
            {
                _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // correct for double click
                var polygon = (Polygon)_addInfo.Feature?.Geometry;
                if (polygon == null) return false;
                polygon.ExteriorRing.Vertices.Add(polygon.ExteriorRing.Vertices.First());
                _addInfo.Feature.RenderedGeometry?.Clear(); // You need to clear the cache to see changes.
                _addInfo.Feature = null;
                _addInfo.Vertex = null;
                EditMode = EditMode.AddPolygon;
                Layer.DataHasChanged();
            }

            return false;
        }

        internal void HoveringVertex(MapInfo mapInfo)
        {
            if (_addInfo.Vertex != null)
            {
                SetPointXY(_addInfo.Vertex, mapInfo.WorldPosition);
                _addInfo.Feature.RenderedGeometry?.Clear();
                Layer.DataHasChanged();
            }
        }
   
        public bool AddVertex(Point worldPosition)
        {
            if (EditMode == EditMode.AddPoint)
            {
                Layer.Add(new Feature { Geometry = worldPosition });
            }
            else if (EditMode == EditMode.AddLine)
            {
                var firstPoint = worldPosition.Clone();
                // Add a second point right away. The second one will be the 'hover' vertex
                var secondPoint = worldPosition.Clone();
                _addInfo.Vertex = secondPoint;
                _addInfo.Feature = new Feature { Geometry = new LineString(new[] { firstPoint, secondPoint }) };
                _addInfo.Vertices = _addInfo.Feature.Geometry.MainVertices();
                Layer.Add(_addInfo.Feature);
                Layer.DataHasChanged();
                EditMode = EditMode.DrawingLine;
            }
            else if (EditMode == EditMode.DrawingLine)
            {
                var lineString = (LineString)_addInfo.Feature.Geometry;
                // Set the final position of the 'hover' vertex (that was already part of the geometry)
                SetPointXY(_addInfo.Vertex, worldPosition.Clone());
                _addInfo.Vertex = worldPosition.Clone(); // and create a new hover vertex
                lineString.Vertices.Add(_addInfo.Vertex); // and add it to the geometry
                _addInfo.Feature.RenderedGeometry?.Clear();
                Layer.DataHasChanged();
            }
            else if (EditMode == EditMode.AddPolygon)
            {
                var firstPoint = worldPosition.Clone();
                // Add a second point right away. The second one will be the 'hover' vertex
                var secondPoint = worldPosition.Clone();
                _addInfo.Vertex = secondPoint;
                _addInfo.Feature = new Feature
                {
                    Geometry = new Polygon
                    {
                        ExteriorRing = new LinearRing(new[] { firstPoint, secondPoint })
                    }
                };
                _addInfo.Vertices = _addInfo.Feature.Geometry.MainVertices();
                Layer.Add(_addInfo.Feature);
                Layer.DataHasChanged();
                EditMode = EditMode.DrawingPolygon;
            }
            else if (EditMode == EditMode.DrawingPolygon)
            {
                var polygon = (Polygon)_addInfo.Feature.Geometry;
                // Set the final position of the 'hover' vertex (that was already part of the geometry)
                SetPointXY(_addInfo.Vertex, worldPosition.Clone());
                _addInfo.Vertex = worldPosition.Clone(); // and create a new hover vertex
                polygon.ExteriorRing.Vertices.Add(_addInfo.Vertex); // and add it to the geometry
                _addInfo.Feature.RenderedGeometry?.Clear();
                Layer.DataHasChanged();
            }
            return false;
        }

        private static Point FindVertexTouched(MapInfo mapInfo, IEnumerable<Point> vertices, double screenDistance)
        {
            return vertices.OrderBy(v => v.Distance(mapInfo.WorldPosition)).FirstOrDefault(v => v.Distance(mapInfo.WorldPosition) < mapInfo.Resolution * screenDistance);
        }
        
        private void SetPointXY(Point target, Point position)
        {
            target.X = position.X;
            target.Y = position.Y;
        }

        public bool StartDragging(MapInfo mapInfo, double screenDistance)
        {
            if (EditMode == EditMode.Modify)
            {
                if (mapInfo.Feature != null)
                {
                    var vertexTouched = FindVertexTouched(mapInfo, mapInfo.Feature.Geometry.MainVertices(), screenDistance);
                    if (vertexTouched != null)
                    {
                        _dragInfo.Feature = mapInfo.Feature;
                        _dragInfo.Vertex = vertexTouched;
                        _dragInfo.StartOffsetToVertex = mapInfo.WorldPosition - _dragInfo.Vertex;

                        return true; // to indicate start of drag
                    }
                }
            }
            return false;
        }
        
        public bool Dragging(Point worldPosition)
        {
            if (EditMode != EditMode.Modify || _dragInfo.Feature == null) return false;
            
            SetPointXY(_dragInfo.Vertex, worldPosition - _dragInfo.StartOffsetToVertex);

            if (_dragInfo.Feature.Geometry is Polygon polygon) // Not this only works correctly it the feature is in the outerring.
            {
                var count = polygon.ExteriorRing.Vertices.Count;
                var vertices = polygon.ExteriorRing.Vertices;
                var index = vertices.IndexOf(_dragInfo.Vertex);
                if (index >= 0)
                    // It is a ring where the first should be the same as the last.
                    // So if the first was removed than set the last to the value of the new first
                    if (index == 0) SetPointXY(vertices[count - 1], vertices[0]);
                    // If the last was removed then set the first to the value of the new last
                    else if (index == vertices.Count) SetPointXY(vertices[0], vertices[count - 1]);
            }

            _dragInfo.Feature.RenderedGeometry.Clear();
            Layer.DataHasChanged();
            return true;
        }

        public void StopDragging()
        {
            if (EditMode == EditMode.Modify && _dragInfo.Feature != null)
            {
                _dragInfo.Feature = null;
            }
        }

        public bool TryDeleteVertrex(MapInfo mapInfo, double screenDistance)
        {
            if (mapInfo.Feature == null) return false;

            var feature = mapInfo.Feature;
            var vertexTouched = FindVertexTouched(mapInfo, feature.Geometry.MainVertices(), screenDistance);
            if (vertexTouched != null)
            {
                var vertices = feature.Geometry.MainVertices();
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

                    feature.RenderedGeometry.Clear();
                    Layer.DataHasChanged();
                }
                
            }
            return false;
        }

        public bool TryInsertVertex(MapInfo mapInfo)
        {
            if (mapInfo.Feature == null) return false;

            var vertices = mapInfo.Feature.Geometry.MainVertices();

            if (EditHelper.TryInsertVertex(mapInfo, vertices, VertexRadius))
            {
                mapInfo.Feature.RenderedGeometry.Clear();
                Layer.DataHasChanged();
            }

            return false;
        }

        public bool StartRotating(MapInfo mapInfo)
        {
            if (EditMode != EditMode.Rotate) return false;
            if (mapInfo.Feature == null) return false;

            _rotateInfo.Feature = mapInfo.Feature;
            _rotateInfo.PreviousPosition = mapInfo.WorldPosition;
            _rotateInfo.Center = mapInfo.Feature.Geometry.BoundingBox.Centroid;

            return true; // to signal pan lock
        }

        public bool Rotating(Point worldPosition)
        {
            if (EditMode != EditMode.Rotate || _rotateInfo.Feature == null) return false;
            
            var previousVector = _rotateInfo.Center - _rotateInfo.PreviousPosition;
            var currentVector = _rotateInfo.Center - worldPosition;
            var degrees = AngleBetween(currentVector, previousVector);

            Geomorpher.Rotate(_rotateInfo.Feature.Geometry, degrees, _rotateInfo.Center);

            _rotateInfo.PreviousPosition = worldPosition;

            _rotateInfo.Feature.RenderedGeometry.Clear();
            Layer.DataHasChanged();
            
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
            double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
            double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

            return Math.Atan2(sin, cos) * (180 / Math.PI);
        }

        public bool StartScaling(MapInfo mapInfo)
        {
            if (EditMode != EditMode.Scale) return false;
            if (mapInfo.Feature == null) return false;

            _scaleInfo.Feature = mapInfo.Feature;
            _scaleInfo.PreviousPosition = mapInfo.WorldPosition;
            _scaleInfo.Center = mapInfo.Feature.Geometry.BoundingBox.Centroid;

            return true; // to signal pan lock
        }

        public bool Scaling(Point worldPosition)
        {
            if (EditMode != EditMode.Scale || _scaleInfo.Feature == null) return false;

            var scale =
                _scaleInfo.Center.Distance(worldPosition) /
                _scaleInfo.Center.Distance(_scaleInfo.PreviousPosition);


            Geomorpher.Scale(_scaleInfo.Feature.Geometry, scale, _scaleInfo.Center);

            _scaleInfo.PreviousPosition = worldPosition;

            _scaleInfo.Feature.RenderedGeometry.Clear();
            Layer.DataHasChanged();

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
