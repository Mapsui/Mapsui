using System;
using Microsoft.Xna.Framework;

namespace Mapsui.Rendering.MonoGame
{
    public static class MatrixHelper
    {
       static readonly Matrix InvertYMatrix = new Matrix { M11 = 1, M21 = -1 };
       static readonly Matrix InvertXMatrix = new Matrix { M11 = -1, M21 = 1 };

       public static void InvertY(ref Matrix matrix, double centerY = 0)
        {
            Translate(ref matrix, 0, -centerY);
            Append(ref matrix, InvertYMatrix);
            Translate(ref matrix, 0, centerY);
        }

        public static void InvertX(ref Matrix matrix, double centerX = 0)
        {
            Translate(ref matrix, -centerX, 0);
            Append(ref matrix, InvertXMatrix);
            Translate(ref matrix, centerX, 0);
        }

        public static void Append(ref Matrix matrix, Matrix matrixOther)
        {
            Multiply(ref matrix, matrixOther);
        } 

        public static void Translate(ref Matrix matrix, double offsetX, double offsetY)
        {
            matrix *= Matrix.CreateTranslation((float)offsetX, (float)offsetY, 0f);
        } 

        public static void ScaleAt(ref Matrix matrix, double scaleX, double scaleY, double centerX = 0, double centerY = 0)
        {
            // this is unlikely to be correct
            matrix *= Matrix.CreateTranslation((float)centerX, (float)centerY, 0f);
            matrix *= Matrix.CreateScale((float)scaleX, (float)scaleY, 0f);
        }

        public static void Multiply(ref Matrix trans1, Matrix trans2)
        {
            trans1 *= trans2;
        }

        //private static void SetMatrix(ref Matrix matrix, double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        //{

        //    matrix.M11 = (float)m11;
        //    matrix.M12 = (float)m12;
        //    matrix.M21 = (float)m21;
        //    matrix.M22 = (float)m22;
        //    matrix.OffsetX = offsetX;
        //    matrix.OffsetY = offsetY;
        //}        

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
            Multiply(ref matrix, CreateRotationRadians(angle * 0.017453292519943295));
        }

        internal static Matrix CreateRotationRadians(double angle)
        {
            return CreateRotationRadians(angle, 0.0, 0.0);
        }

        internal static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
        {
            var matrix = new Matrix(); 
            var num = (float)Math.Sin(angle);
            var num2 = (float)Math.Cos(angle);
            float offsetX = ((float)centerX * (1f - num2)) + ((float)centerY * num);
            float offsetY = ((float)centerY * (1f - num2)) - ((float)centerX * num);
            matrix.M11 = num2;
            matrix.M12 = num;
            matrix.M21 = -num;
            matrix.M22 = num2;
            matrix.Translation = new Vector3(offsetX, offsetY, 0f);
            return matrix;
        }

        public static void RotateAt(ref Matrix matrix, double angle, double centerX = 0, double centerY = 0)
        {
            angle = angle % 360.0;
            Multiply(ref matrix, CreateRotationRadians(angle * 0.017453292519943295, centerX, centerY));
        }
    }
}
