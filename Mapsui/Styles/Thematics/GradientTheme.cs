// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using SharpMap.Providers;

namespace SharpMap.Styles.Thematics
{
    /// <summary>
    /// The GradientTheme class defines a gradient color thematic rendering of features based by a numeric attribute.
    /// </summary>
    public class GradientTheme : Style, IThemeStyle
    {
        #region Fields
        private string columnName;
        private double minVal;
        private double maxVal;
        private IStyle minStyle;
        private IStyle maxStyle;
        private ColorBlend textColorBlend;
        private ColorBlend lineColorBlend;
        private ColorBlend fillColorBlend;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the column name from where to get the attribute value
        /// </summary>
        public string ColumnName
        {
            get { return columnName; }
            set { columnName = value; }
        }

        /// <summary>
        /// Gets or sets the minimum value of the gradient
        /// </summary>
        public double Min
        {
            get { return minVal; }
            set { minVal = value; }
        }

        /// <summary>
        /// Gets or sets the maximum value of the gradient
        /// </summary>
        public double Max
        {
            get { return maxVal; }
            set { maxVal = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.IStyle">style</see> for the minimum value
        /// </summary>
        public IStyle MinStyle
        {
            get { return minStyle; }
            set { minStyle = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.IStyle">style</see> for the maximum value
        /// </summary>
        public IStyle MaxStyle
        {
            get { return maxStyle; }
            set { maxStyle = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.Thematics.ColorBlend"/> used on labels
        /// </summary>
        public ColorBlend TextColorBlend
        {
            get { return textColorBlend; }
            set { textColorBlend = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.Thematics.ColorBlend"/> used on lines
        /// </summary>
        public ColorBlend LineColorBlend
        {
            get { return lineColorBlend; }
            set { lineColorBlend = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.Thematics.ColorBlend"/> used as Fill
        /// </summary>
        public ColorBlend FillColorBlend
        {
            get { return fillColorBlend; }
            set { fillColorBlend = value; }
        }

#endregion

        /// <summary>
        /// Initializes a new instance of the GradientTheme class
        /// </summary>
        /// <remarks>
        /// <para>The gradient theme interpolates linearly between two styles based on a numerical attribute in the datasource.
        /// This is useful for scaling symbols, line widths, line and fill colors from numerical attributes.</para>
        /// <para>Colors are interpolated between two colors, but if you want to interpolate through more colors (fx. a rainbow),
        /// set the <see cref="TextColorBlend"/>, <see cref="LineColorBlend"/> and <see cref="FillColorBlend"/> properties
        /// to a custom <see cref="ColorBlend"/>.
        /// </para>
        /// <para>The following properties are scaled (properties not mentioned here are not interpolated):
        /// <list type="table">
        ///        <listheader><term>Property</term><description>Remarks</description></listheader>
        ///        <item><term><see cref="Color"/></term><description>Red, Green, Blue and Alpha values are linearly interpolated.</description></item>
        ///        <item><term><see cref="Pen"/></term><description>The color, width, color of pens are interpolated. MiterLimit,StartCap,EndCap,LineJoin,DashStyle,DashPattern,DashOffset,DashCap,CompoundArray, and Alignment are switched in the middle of the min/max values.</description></item>
        ///        <item><term><see cref="Brush"/></term><description>Brush color are interpolated. Other brushes are not supported.</description></item>
        ///        <item><term><see cref="SharpMap.Styles.VectorStyle"/></term><description>MaxVisible, MinVisible, Line, Outline, Fill and SymbolScale are scaled linearly. Symbol, EnableOutline and Enabled switch in the middle of the min/max values.</description></item>
        ///        <item><term><see cref="SharpMap.Styles.LabelStyle"/></term><description>FontSize, BackColor, ForeColor, MaxVisible, MinVisible, Offset are scaled linearly. All other properties use min-style.</description></item>
        /// </list>
        /// </para>
        /// <example>
        /// Creating a rainbow colorblend showing colors from red, through yellow, green and blue depicting 
        /// the population density of a country.
        /// <code lang="C#">
        /// //Create two vector styles to interpolate between
        /// SharpMap.Styles.VectorStyle min = new SharpMap.Styles.VectorStyle();
        /// SharpMap.Styles.VectorStyle max = new SharpMap.Styles.VectorStyle();
        /// min.Outline.Width = 1f; //Outline width of the minimum value
        /// max.Outline.Width = 3f; //Outline width of the maximum value
        /// //Create a theme interpolating population density between 0 and 400
        /// SharpMap.Rendering.Thematics.GradientTheme popdens = new SharpMap.Rendering.Thematics.GradientTheme("PopDens", 0, 400, min, max);
        /// //Set the fill-style colors to be a rainbow blend from red to blue.
        /// popdens.FillColorBlend = SharpMap.Rendering.Thematics.ColorBlend.Rainbow5;
        /// myVectorLayer.Styles.Add(popdens);
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="columnName">Name of column to extract the attribute</param>
        /// <param name="minValue">Minimum value</param>
        /// <param name="maxValue">Maximum value</param>
        /// <param name="minStyle">Color for minimum value</param>
        /// <param name="maxStyle">Color for maximum value</param>
        public GradientTheme(string columnName, double minValue, double maxValue, IStyle minStyle, IStyle maxStyle)
        {
            this.columnName = columnName;
            minVal = minValue;
            maxVal = maxValue;
            this.maxStyle = maxStyle;
            this.minStyle = minStyle;
        }

        #region ITheme Members

        /// <summary>
        /// Returns the style based on a numeric DataColumn, where style
        /// properties are linearly interpolated between max and min values.
        /// </summary>
        /// <param name="row">Feature</param>
        /// <returns><see cref="SharpMap.Styles.IStyle">Style</see> calculated by a linear interpolation between the min/max styles</returns>
        public IStyle GetStyle(IFeature row)
        {
            double attr;
            try { attr = Convert.ToDouble(row[columnName.ToUpper()]); }
            catch { throw new Exception("Invalid Attribute type in Gradient Theme - Couldn't parse attribute (must be numerical)"); }
            if (minStyle.GetType() != maxStyle.GetType())
                throw new ArgumentException("MinStyle and MaxStyle must be of the same type");


            var style = (IStyle)Activator.CreateInstance(MinStyle.GetType());
            if (MinStyle is LabelStyle && MaxStyle is LabelStyle)
                CalculateLabelStyle(style as LabelStyle, MinStyle as LabelStyle, MaxStyle as LabelStyle, attr);
            if (MinStyle is VectorStyle && MaxStyle is VectorStyle) 
                CalculateVectorStyle(style as VectorStyle, MinStyle as VectorStyle, MaxStyle as VectorStyle, attr);
            if (MinStyle is SymbolStyle && MaxStyle is SymbolStyle)
                CalculateSymbolStyle(style as SymbolStyle, MinStyle as SymbolStyle, MaxStyle as SymbolStyle, attr);
            return style;
        }

        private void CalculateVectorStyle(VectorStyle style, VectorStyle min, VectorStyle max, double value)
        {
            double dFrac = Fraction(value);
            double fFrac = Convert.ToSingle(dFrac);
            style.Enabled = (dFrac > 0.5 ? min.Enabled : max.Enabled);
            if (fillColorBlend != null)
                style.Fill = new Brush { Color = fillColorBlend.GetColor(fFrac) };
            else if (min.Fill != null && max.Fill != null)
                style.Fill = InterpolateBrush(min.Fill, max.Fill, value);

            if (min.Line != null && max.Line != null)
                style.Line = InterpolatePen(min.Line, max.Line, value);
            if (lineColorBlend != null)
                style.Line.Color = lineColorBlend.GetColor(fFrac);

            if (min.Outline != null && max.Outline != null)
                style.Outline = InterpolatePen(min.Outline, max.Outline, value);
        }

        private void CalculateSymbolStyle(SymbolStyle style, SymbolStyle min, SymbolStyle max, double value)
        {
            double dFrac = Fraction(value);
            style.Symbol = (dFrac > 0.5 ? min.Symbol : max.Symbol);
            style.SymbolOffset = (dFrac > 0.5 ? min.SymbolOffset : max.SymbolOffset);
            //We don't interpolate the offset but let it follow the symbol instead
            style.SymbolScale = InterpolateDouble(min.SymbolScale, max.SymbolScale, value);
        }

        private void CalculateLabelStyle(LabelStyle style, LabelStyle min, LabelStyle max, double value)
        {
            style.CollisionDetection = min.CollisionDetection;
            style.Enabled = InterpolateBool(min.Enabled, max.Enabled, value);

            double fontSize = InterpolateDouble(min.Font.Size, max.Font.Size, value);
            style.Font = new Font { FontFamily = min.Font.FontFamily, Size = fontSize };

            if (min.BackColor != null && max.BackColor != null)
                style.BackColor = InterpolateBrush(min.BackColor, max.BackColor, value);

            if (textColorBlend != null)
                style.ForeColor = lineColorBlend.GetColor(Convert.ToSingle(Fraction(value)));
            else
                style.ForeColor = InterpolateColor(min.ForeColor, max.ForeColor, value);
            if (min.Halo != null && max.Halo != null)
                style.Halo = InterpolatePen(min.Halo, max.Halo, value);

            var x = InterpolateDouble(min.Offset.X, max.Offset.X, value);
            var y = InterpolateDouble(min.Offset.Y, max.Offset.Y, value);
            style.Offset = new Offset { X = x, Y = y };
        }

        private double Fraction(double attr)
        {
            if (attr < minVal) return 0;
            if (attr > maxVal) return 1;
            return (attr - minVal) / (maxVal - minVal);
        }

        private bool InterpolateBool(bool min, bool max, double attr)
        {
            double frac = Fraction(attr);
            if (frac > 0.5) return max;
            return min;
        }

        private double InterpolateFloat(float min, float max, double attr)
        {
            return Convert.ToSingle((max - min) * Fraction(attr) + min);
        }

        private double InterpolateDouble(double min, double max, double attr)
        {
            return (max - min) * Fraction(attr) + min;
        }

        private Brush InterpolateBrush(Brush min, Brush max, double attr)
        {
            if (min.GetType() != typeof(Brush) || max.GetType() != typeof(Brush))
                throw (new ArgumentException("Only Brush brushes are supported in GradientTheme"));

            return new Brush { Color = InterpolateColor(min.Color, max.Color, attr) };
        }

        private Pen InterpolatePen(Pen min, Pen max, double attr)
        {
            return new Pen
            {
                Color = InterpolateColor(min.Color, max.Color, attr),
                Width = InterpolateDouble(min.Width, max.Width, attr)
            };

        }

        private Color InterpolateColor(Color minCol, Color maxCol, double attr)
        {
            double frac = Fraction(attr);
            if (frac == 1)
                return maxCol;
            if (frac == 0)
                return minCol;

            double r = (maxCol.R - minCol.R) * frac + minCol.R;
            double g = (maxCol.G - minCol.G) * frac + minCol.G;
            double b = (maxCol.B - minCol.B) * frac + minCol.B;
            double a = (maxCol.A - minCol.A) * frac + minCol.A;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            if (a > 255) a = 255;

            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }
        #endregion
    }
}
