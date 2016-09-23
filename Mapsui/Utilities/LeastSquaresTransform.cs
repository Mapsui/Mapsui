// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.Utilities
{
    /// <summary>
    /// Calculates Affine and Helmert transformation using Least-Squares Regression of input and output points
    /// </summary>
    public class LeastSquaresTransform
    {
        private readonly List<Point> _inputs;
        private readonly List<Point> _outputs;

        /// <summary>
        /// Initialize Least Squares transformations
        /// </summary>
        public LeastSquaresTransform()
        {
            _inputs = new List<Point>();
            _outputs = new List<Point>();
        }

        /// <summary>
        /// Adds an input and output value pair to the collection
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void AddInputOutputPoint(Point input, Point output)
        {
            _inputs.Add(input);
            _outputs.Add(output);
        }

        /// <summary>
        /// Removes input and output value pair at the specified index
        /// </summary>
        /// <param name="i"></param>
        public void RemoveInputOutputPointAt(int i)
        {
            _inputs.RemoveAt(i);
            _outputs.RemoveAt(i);
        }

        /// <summary>
        /// Gets the input point value at the specified index
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>Input point value a index 'i'</returns>
        public Point GetInputPoint(int i)
        {
            return _inputs[i];
        }

        /// <summary>
        /// Sets the input point value at the specified index
        /// </summary>
        /// <param name="p">Point value</param>
        /// <param name="i">index</param>
        public void SetInputPointAt(Point p, int i)
        {
            _inputs[i] = p;
        }

        /// <summary>
        /// Gets the output point value at the specified index
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>Output point value a index 'i'</returns>
        public Point GetOutputPoint(int i)
        {
            return _outputs[i];
        }

        /// <summary>
        /// Sets the output point value at the specified index
        /// </summary>
        /// <param name="p">Point value</param>
        /// <param name="i">index</param>
        public void SetOutputPointAt(Point p, int i)
        {
            _outputs[i] = p;
        }

        /// <summary>
        /// Return an array with the six affine transformation parameters {a,b,c,d,e,f} and the sum of the squares of the residuals (s0)
        /// </summary>
        /// <remarks>
        /// a,b defines scale vector 1 of coordinate system, d,e scale vector 2. c,f defines offset.
        /// <para>
        /// Converting from input (X,Y) to output coordinate system (X',Y') is done by:
        /// X' = a*X + b*Y + c, Y' = d*X + e*Y + f
        /// </para>
        /// <para>
        /// Transformation based on Mikhail "Introduction to Modern Photogrammetry" p. 399-300.
        /// Extended to arbitrary number of measurements by M. Nielsen
        /// </para>
        /// </remarks>
        /// <returns>Array with the six transformation parameters and sum of squared residuals:  a,b,c,d,e,f,s0</returns>
        public double[] GetAffineTransformation()
        {
            if (_inputs.Count < 3)
                throw (new Exception("At least 3 measurements required to calculate affine transformation"));

            //double precision isn't always enough when transforming large numbers.
            //Lets subtract some mean values and add them later again:
            //Find approximate center values:
            var meanInput = new Point(0, 0);
            var meanOutput = new Point(0, 0);
            for (int i = 0; i < _inputs.Count; i++)
            {
                meanInput.X += _inputs[i].X;
                meanInput.Y += _inputs[i].Y;
                meanOutput.X += _outputs[i].X;
                meanOutput.Y += _outputs[i].Y;
            }
            meanInput.X = Math.Round(meanInput.X/_inputs.Count);
            meanInput.Y = Math.Round(meanInput.Y/_inputs.Count);
            meanOutput.X = Math.Round(meanOutput.X/_inputs.Count);
            meanOutput.Y = Math.Round(meanOutput.Y/_inputs.Count);

            double[][] n = CreateMatrix(3, 3);
            //Create normal equation: transpose(B)*B
            //B: matrix of calibrated values. Example of row in B: [x , y , -1]
            for (int i = 0; i < _inputs.Count; i++)
            {
                //Subtract mean values
                _inputs[i].X -= meanInput.X;
                _inputs[i].Y -= meanInput.Y;
                _outputs[i].X -= meanOutput.X;
                _outputs[i].Y -= meanOutput.Y;
                //Calculate summed values
                n[0][0] += Math.Pow(_inputs[i].X, 2);
                n[0][1] += _inputs[i].X*_inputs[i].Y;
                n[0][2] += -_inputs[i].X;
                n[1][1] += Math.Pow(_inputs[i].Y, 2);
                n[1][2] += -_inputs[i].Y;
            }
            n[2][2] = _inputs.Count;

            var t1 = new double[3];
            var t2 = new double[3];

            for (int i = 0; i < _inputs.Count; i++)
            {
                t1[0] += _inputs[i].X*_outputs[i].X;
                t1[1] += _inputs[i].Y*_outputs[i].X;
                t1[2] += -_outputs[i].X;

                t2[0] += _inputs[i].X*_outputs[i].Y;
                t2[1] += _inputs[i].Y*_outputs[i].Y;
                t2[2] += -_outputs[i].Y;
            }
            var trans = new double[7];
            // Solve equation N = transpose(B)*t1
            double frac = 1/
                          (-n[0][0]*n[1][1]*n[2][2] + n[0][0]*Math.Pow(n[1][2], 2) + Math.Pow(n[0][1], 2)*n[2][2] -
                           2*n[1][2]*n[0][1]*n[0][2] + n[1][1]*Math.Pow(n[0][2], 2));
            trans[0] = (-n[0][1]*n[1][2]*t1[2] + n[0][1]*t1[1]*n[2][2] - n[0][2]*n[1][2]*t1[1] + n[0][2]*n[1][1]*t1[2] -
                        t1[0]*n[1][1]*n[2][2] + t1[0]*Math.Pow(n[1][2], 2))*frac;
            trans[1] = (-n[0][1]*n[0][2]*t1[2] + n[0][1]*t1[0]*n[2][2] + n[0][0]*n[1][2]*t1[2] - n[0][0]*t1[1]*n[2][2] -
                        n[0][2]*n[1][2]*t1[0] + Math.Pow(n[0][2], 2)*t1[1])*frac;
            trans[2] =
                -(-n[1][2]*n[0][1]*t1[0] + Math.Pow(n[0][1], 2)*t1[2] + n[0][0]*n[1][2]*t1[1] - n[0][0]*n[1][1]*t1[2] -
                  n[0][2]*n[0][1]*t1[1] + n[1][1]*n[0][2]*t1[0])*frac;
            trans[2] += - meanOutput.X + meanInput.X;
            // Solve equation N = transpose(B)*t2
            trans[3] = (-n[0][1]*n[1][2]*t2[2] + n[0][1]*t2[1]*n[2][2] - n[0][2]*n[1][2]*t2[1] + n[0][2]*n[1][1]*t2[2] -
                        t2[0]*n[1][1]*n[2][2] + t2[0]*Math.Pow(n[1][2], 2))*frac;
            trans[4] = (-n[0][1]*n[0][2]*t2[2] + n[0][1]*t2[0]*n[2][2] + n[0][0]*n[1][2]*t2[2] - n[0][0]*t2[1]*n[2][2] -
                        n[0][2]*n[1][2]*t2[0] + Math.Pow(n[0][2], 2)*t2[1])*frac;
            trans[5] =
                -(-n[1][2]*n[0][1]*t2[0] + Math.Pow(n[0][1], 2)*t2[2] + n[0][0]*n[1][2]*t2[1] - n[0][0]*n[1][1]*t2[2] -
                  n[0][2]*n[0][1]*t2[1] + n[1][1]*n[0][2]*t2[0])*frac;
            trans[5] += - meanOutput.Y + meanInput.Y;

            //Restore values
            for (int i = 0; i < _inputs.Count; i++)
            {
                _inputs[i].X += meanInput.X;
                _inputs[i].Y += meanInput.Y;
                _outputs[i].X += meanOutput.X;
                _outputs[i].Y += meanOutput.Y;
            }

            //Calculate s0
            double s0 = 0;
            for (int i = 0; i < _inputs.Count; i++)
            {
                double x = _inputs[i].X*trans[0] + _inputs[i].Y*trans[1] + trans[2];
                double y = _inputs[i].X*trans[3] + _inputs[i].Y*trans[4] + trans[5];
                s0 += Math.Pow(x - _outputs[i].X, 2) + Math.Pow(y - _outputs[i].Y, 2);
            }
            trans[6] = Math.Sqrt(s0)/(_inputs.Count);
            return trans;
        }

        /// <summary>
        /// Calculates the four helmert transformation parameters {a,b,c,d} and the sum of the squares of the residuals (s0)
        /// </summary>
        /// <remarks>
        /// <para>
        /// a,b defines scale vector 1 of coordinate system, d,e scale vector 2.
        /// c,f defines offset.
        /// </para>
        /// <para>
        /// Converting from input (X,Y) to output coordinate system (X',Y') is done by:
        /// X' = a*X + b*Y + c, Y' = -b*X + a*Y + d
        /// </para>
        /// <para>This is a transformation initially based on the affine transformation but slightly simpler.</para>
        /// </remarks>
        /// <returns>Array with the four transformation parameters, and sum of squared residuals: a,b,c,d,s0</returns>
        public double[] GetHelmertTransformation()
        {
            if (_inputs.Count < 2)
                throw (new Exception("At least 2 measurements required to calculate helmert transformation"));

            //double precision isn't always enough. Lets subtract some mean values and add them later again:
            //Find approximate center values:
            var meanInput = new Point(0, 0);
            var meanOutput = new Point(0, 0);
            for (int i = 0; i < _inputs.Count; i++)
            {
                meanInput.X += _inputs[i].X;
                meanInput.Y += _inputs[i].Y;
                meanOutput.X += _outputs[i].X;
                meanOutput.Y += _outputs[i].Y;
            }
            meanInput.X = Math.Round(meanInput.X/_inputs.Count);
            meanInput.Y = Math.Round(meanInput.Y/_inputs.Count);
            meanOutput.X = Math.Round(meanOutput.X/_inputs.Count);
            meanOutput.Y = Math.Round(meanOutput.Y/_inputs.Count);

            double b00 = 0;
            double b02 = 0;
            double b03 = 0;
            var t = new double[4];
            for (int i = 0; i < _inputs.Count; i++)
            {
                //Subtract mean values
                _inputs[i].X -= meanInput.X;
                _inputs[i].Y -= meanInput.Y;
                _outputs[i].X -= meanOutput.X;
                _outputs[i].Y -= meanOutput.Y;
                //Calculate summed values
                b00 += Math.Pow(_inputs[i].X, 2) + Math.Pow(_inputs[i].Y, 2);
                b02 -= _inputs[i].X;
                b03 -= _inputs[i].Y;
                t[0] += -(_inputs[i].X*_outputs[i].X) - (_inputs[i].Y*_outputs[i].Y);
                t[1] += -(_inputs[i].Y*_outputs[i].X) + (_inputs[i].X*_outputs[i].Y);
                t[2] += _outputs[i].X;
                t[3] += _outputs[i].Y;
            }
            double frac = 1/(-_inputs.Count*b00 + Math.Pow(b02, 2) + Math.Pow(b03, 2));
            var result = new double[5];
            result[0] = (-_inputs.Count*t[0] + b02*t[2] + b03*t[3])*frac;
            result[1] = (-_inputs.Count*t[1] + b03*t[2] - b02*t[3])*frac;
            result[2] = (b02*t[0] + b03*t[1] - t[2]*b00)*frac + meanOutput.X;
            result[3] = (b03*t[0] - b02*t[1] - t[3]*b00)*frac + meanOutput.Y;

            //Restore values
            for (int i = 0; i < _inputs.Count; i++)
            {
                _inputs[i].X += meanInput.X;
                _inputs[i].Y += meanInput.Y;
                _outputs[i].X += meanOutput.X;
                _outputs[i].Y += meanOutput.Y;
            }

            //Calculate s0
            double s0 = 0;
            for (int i = 0; i < _inputs.Count; i++)
            {
                double x = _inputs[i].X*result[0] + _inputs[i].Y*result[1] + result[2];
                double y = -_inputs[i].X*result[1] + _inputs[i].Y*result[0] + result[3];
                s0 += Math.Pow(x - _outputs[i].X, 2) + Math.Pow(y - _outputs[i].Y, 2);
            }
            result[4] = Math.Sqrt(s0)/(_inputs.Count);
            return result;
        }

        /// <summary>
        /// Creates an n x m matrix of doubles
        /// </summary>
        /// <param name="n">width of matrix</param>
        /// <param name="m">height of matrix</param>
        /// <returns>n*m matrix</returns>
        private double[][] CreateMatrix(int n, int m)
        {
            var matrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                matrix[i] = new double[m];
            }
            return matrix;
        }
    }
}