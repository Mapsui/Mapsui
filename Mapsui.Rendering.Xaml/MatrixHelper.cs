using System;
using Mapsui.Geometries.Utilities;
using System.Windows.Media;

namespace Mapsui.Rendering.Xaml
{
    public static class MatrixHelper
    {
       public static void InvertY(ref Matrix matrix, double centerY = 0)
        {
            Translate(ref matrix, 0, -centerY);
            Append(ref matrix, new Matrix(1, 0, 0, -1, 0, 0));
            Translate(ref matrix, 0, centerY);
        }

        public static void InvertX(ref Matrix matrix, double centerX = 0)
        {
            Translate(ref matrix, -centerX, 0);
            Append(ref matrix, new Matrix(-1, 0, 0, 1, 0, 0));
            Translate(ref matrix, centerX, 0);
        }

        public static void Append(ref Matrix matrix, Matrix matrixOther)
        {
            Multiply(ref matrix, matrixOther);
        } 

        public static void Translate(ref Matrix matrix, double offsetX, double offsetY)
        {
            matrix.OffsetX += offsetX;
            matrix.OffsetY += offsetY;
        } 

        public static void ScaleAt(ref Matrix matrix, double scaleX, double scaleY, double centerX = 0, double centerY = 0)
        {
             Multiply(ref matrix, CreateScaling(scaleX, scaleY, centerX, centerY));
        }

        public static void Multiply(ref Matrix trans1, Matrix trans2)
        {
            MultiplyMatrix(ref trans1, ref trans2);
        }

        internal static void MultiplyMatrix(ref Matrix matrix1, ref Matrix matrix2)
        {
            const MatrixTypes types = MatrixTypes.TransformIsUnknown;
            const MatrixTypes types2 = MatrixTypes.TransformIsScaling | MatrixTypes.TransformIsTranslation;
            int thisCase = ((((int)types) << 4) | ((int)types2));

            switch (thisCase)
            {
                case 0x22:
                    matrix1.M11 *= matrix2.M11;
                    matrix1.M22 *= matrix2.M22;
                    return;

                case 0x23:
                    matrix1.M11 *= matrix2.M11;
                    matrix1.M22 *= matrix2.M22;
                    matrix1.OffsetX = matrix2.OffsetX;
                    matrix1.OffsetY = matrix2.OffsetY;
                    return;

                case 0x24:
                case 0x34:
                case 0x42:
                case 0x43:
                case 0x44:
                    matrix1 = new Matrix((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21), (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22), (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21), (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22), ((matrix1.OffsetX * matrix2.M11) + (matrix1.OffsetY * matrix2.M21)) + matrix2.OffsetX, ((matrix1.OffsetX * matrix2.M12) + (matrix1.OffsetY * matrix2.M22)) + matrix2.OffsetY);
                    return;

                case 50:
                    matrix1.M11 *= matrix2.M11;
                    matrix1.M22 *= matrix2.M22;
                    matrix1.OffsetX *= matrix2.M11;
                    matrix1.OffsetY *= matrix2.M22;
                    return;

                case 0x33:
                    matrix1.M11 *= matrix2.M11;
                    matrix1.M22 *= matrix2.M22;
                    matrix1.OffsetX = (matrix2.M11 * matrix1.OffsetX) + matrix2.OffsetX;
                    matrix1.OffsetY = (matrix2.M22 * matrix1.OffsetY) + matrix2.OffsetY;
                    return;  
            }
        }
        
        public static Matrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
        {
            var matrix = new Matrix(); 
            SetMatrix(ref matrix, scaleX, 0.0, 0.0, scaleY, centerX - (scaleX * centerX), centerY - (scaleY * centerY));
            return matrix;
        }

        private static void SetMatrix(ref Matrix matrix, double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            matrix.M11 = m11;
            matrix.M12 = m12;
            matrix.M21 = m21;
            matrix.M22 = m22;
            matrix.OffsetX = offsetX;
            matrix.OffsetY = offsetY;
        }        

        [Flags]
        internal enum MatrixTypes 
        {
            TransformIsIdentity = 0,
            TransformIsScaling = 2,
            TransformIsTranslation = 1,
            TransformIsUnknown = 4
        }

        public static void Rotate(ref Matrix matrix, double angle)
        {
            angle = angle % 360.0;
            var radians = Algorithms.DegreesToRadians(angle);
            Multiply(ref matrix, CreateRotationRadians(radians));
        }

        internal static Matrix CreateRotationRadians(double angle)
        {
            return CreateRotationRadians(angle, 0.0, 0.0);
        }

        internal static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
        {
            var matrix = new Matrix();
            double num = Math.Sin(angle);
            double num2 = Math.Cos(angle);
            double offsetX = (centerX * (1.0 - num2)) + (centerY * num);
            double offsetY = (centerY * (1.0 - num2)) - (centerX * num);
            SetMatrix(ref matrix, num2, num, -num, num2, offsetX, offsetY);
            return matrix;
        }

        public static void RotateAt(ref Matrix matrix, double angle, double centerX = 0, double centerY = 0)
        {
            angle = angle % 360.0;
            var radians = Algorithms.DegreesToRadians(angle);
            Multiply(ref matrix, CreateRotationRadians(radians, centerX, centerY));
        }
    }
}
