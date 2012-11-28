using System;
using System.Collections.Generic;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap.Geometries;

namespace SharpMap.Projection
{
    public static class ProjectionHelper
    {
        public static void Transform(IGeometry geometry, ICoordinateTransformation coordinateTransformation)
        {
            var vertices = AllVertices(geometry);
            foreach (var vertex in vertices)
            {
                double[] point = coordinateTransformation.MathTransform.Transform(new[] { vertex.X, vertex.Y });
                vertex.X = point[0];
                vertex.Y = point[1];
            }
        }

        public static void InverseTransform(IGeometry geometry, ICoordinateTransformation coordinateTransformation)
        {
            var vertices = AllVertices(geometry);
            foreach (var vertex in vertices)
            {
                coordinateTransformation.MathTransform.Inverse();
                double[] point = coordinateTransformation.MathTransform.Transform(new[] { vertex.X, vertex.Y });
                coordinateTransformation.MathTransform.Inverse();
                vertex.X = point[0];
                vertex.Y = point[1];
            }
        }

        public static BoundingBox Transform(BoundingBox box, ICoordinateTransformation coordinateTransformation)
        {
            double[] point1 = coordinateTransformation.MathTransform.Transform(new[] { box.MinX, box.MinY });
            double[] point2 = coordinateTransformation.MathTransform.Transform(new[] { box.MaxX, box.MaxY });
            return new BoundingBox(point1[0], point1[1], point2[0], point2[1]);
        }

        public static BoundingBox InverseTransform(BoundingBox box, ICoordinateTransformation coordinateTransformation)
        {
            coordinateTransformation.MathTransform.Invert();
            double[] point1 = coordinateTransformation.MathTransform.Transform(new[] { box.MinX, box.MinY });
            double[] point2 = coordinateTransformation.MathTransform.Transform(new[] { box.MaxX, box.MaxY });
            coordinateTransformation.MathTransform.Invert();
            return new BoundingBox(point1[0], point1[1], point2[0], point2[1]);
        }
    }
}
