using Mapsui.Geometries;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.ScaleBar
{
    ///
    /// A ScaleBarOverlay displays the ratio of a distance on the map to the corresponding distance on the ground.
    ///
    public class ScaleBarWidget : Widget, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ///
        /// Default position of the scale bar.
        ///
        private static readonly HorizontalAlignment DefaultScaleBarHorizontalAlignment = HorizontalAlignment.Left;
        private static readonly VerticalAlignment DefaultScaleBarVerticalAlignment = VerticalAlignment.Bottom;
        private static readonly Alignment DefaultScaleBarAlignment = Alignment.Left;
        private static readonly ScaleBarMode DefaultScaleBarMode = ScaleBarMode.Single;

        protected IUnitConverter unitConverter;
        protected IUnitConverter secondaryUnitConverter;
        protected ScaleBarMode scaleBarMode;
        Color textColor = new Color(0, 0, 0);
        Color backColor = new Color(255, 255, 255);
        float maxWidth;
        float height;
        Alignment textAlignment;
        double lastResolution = double.MaxValue;

        public ScaleBarWidget()
        {
            HorizontalAlignment = DefaultScaleBarHorizontalAlignment;
            VerticalAlignment = DefaultScaleBarVerticalAlignment;

            maxWidth = 100;
            height = 100;
            textAlignment = DefaultScaleBarAlignment;
            scaleBarMode = DefaultScaleBarMode;

            unitConverter = MetricUnitConverter.Instance;

            Font = new Font { FontFamily = "Arial", Size = 10 };
        }

        public Viewport Viewport { get; set; } = null;

        /// <summary>
        /// Alignment of text of scale bar
        /// </summary>
        public Alignment TextAlignment
        {
            get
            {
                return textAlignment;
            }
            set
            {
                if (textAlignment == value)
                    return;

                textAlignment = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Maximum width of scale bar
        /// </summary>
        public float MaxWidth
        {
            get
            {
                return maxWidth;
            }
            set
            {
                if (maxWidth == value)
                    return;

                maxWidth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Real height of scale bar. Depends on unit converters.
        /// </summary>
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height == value)
                    return;

                height = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Foreground color of scale bar and text
        /// </summary>
        public Color TextColor
        {
            get
            {
                return textColor;
            }
            set
            {
                if (textColor == value)
                    return;
                textColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Background color of scale bar and text
        /// </summary>
        public Color BackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                if (backColor == value)
                    return;
                backColor = value;
                OnPropertyChanged();
            }
        }

        public float Scale { get; set; } = 1;

        /// <summary>
        /// Length of the ticks
        /// </summary>
        public float TickLength { get; set; } = 3;

        /// <summary>
        /// Margin between end of tick and text
        /// </summary>
        public float TextMargin { get; set; } = 1;

        /// <summary>
        /// Font to use for drawing text
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Normal unit converter for upper text. Default is MetricUnitConverter.
        /// </summary>
        public IUnitConverter UnitConverter
        {
            get { return unitConverter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("UnitConverter must not be null");
                }
                if (unitConverter == value)
                {
                    return;
                }

                unitConverter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Draw a rectangle around the scale bar for testing
        /// </summary>
        public bool ShowEnvelop { get; set; } = false;

        /// <summary>
        /// Secondary unit converter for lower text if ScaleBarMode is Both. Default is ImperialUnitConverter.
        /// </summary>
        public IUnitConverter SecondaryUnitConverter
        {
            get { return secondaryUnitConverter; }
            set
            {
                if (secondaryUnitConverter == value)
                {
                    return;
                }

                secondaryUnitConverter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ScaleBarMode of scale bar. Could be Single to show only one or Both for showing two units.
        /// </summary>
        public ScaleBarMode ScaleBarMode
        {
            get
            {
                return scaleBarMode;
            }
            set
            {
                if (scaleBarMode == value)
                {
                    return;
                }

                scaleBarMode = value;
                OnPropertyChanged();
            }
        }

        public float CalculatePositionX(float left, float right, float width)
        {
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    return MarginX;

                case HorizontalAlignment.Center:
                    return (right - left - width) / 2;

                case HorizontalAlignment.Right:
                    return right - left - width - MarginX;

                case HorizontalAlignment.Position:
                    return PositionX;
            }

            throw new ArgumentException("Unknown horizontal alignment: " + HorizontalAlignment);
        }

        public float CalculatePositionY(float top, float bottom, float height)
        {
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    return MarginY;

                case VerticalAlignment.Bottom:
                    return bottom - top - height - MarginY;

                case VerticalAlignment.Center:
                    return (bottom - top - height) / 2;

                case VerticalAlignment.Position:
                    return PositionY;
            }

            throw new ArgumentException("Unknown vertical alignment: " + VerticalAlignment);
        }

        /// Calculates the required length and value of the scalebar
        ///
        /// @param viewport the Viewport to calculate for
        /// @param width of the scale bar in pixel to calculate for
        /// @param unitConverter the DistanceUnitConverter to calculate for
        /// @return scaleBarLength, mapScaleValue and mapScaleText
        public (float scaleBarLength, int mapScaleValue, string mapScaleText) CalculateScaleBarLengthAndValue(IViewport viewport, float width, IUnitConverter unitConverter)
        {
            // Get current position
            var position = Projection.SphericalMercator.ToLonLat(viewport.Center.X, viewport.Center.Y);

            // Calc ground resolution in meters per pixel of viewport for this latitude
            double groundResolution = viewport.Resolution * Math.Cos(position.Y / 180.0 * Math.PI);

            // Convert in units of UnitConverter
            groundResolution = groundResolution / unitConverter.MeterRatio;

            int[] scaleBarValues = unitConverter.ScaleBarValues;

            float scaleBarLength = 0;
            int mapScaleValue = 0;

            foreach (int scaleBarValue in scaleBarValues)
            {
                mapScaleValue = scaleBarValue;
                scaleBarLength = (float)(mapScaleValue / groundResolution);
                if (scaleBarLength < (width - 10))
                {
                    break;
                }
            }

            var mapScaleText = unitConverter.GetScaleText(mapScaleValue);

            return (scaleBarLength, mapScaleValue, mapScaleText);
        }

        /**
		 * Calculates the required length and value of the scalebar using the current {@link DistanceUnitAdapter}
		 *
		 * @return a {@link ScaleBarLengthAndValue} object containing the required scaleBarLength and scaleBarValue
		 */
        public (float scaleBarLength, int mapScaleValue, string mapScaleText) CalculateScaleBarLengthAndValue(IViewport viewport, float width)
        {
            return CalculateScaleBarLengthAndValue(viewport, width, UnitConverter);
        }

        public Point[] DrawLines(float scaleBarLength1, float scaleBarLength2, float stroke)
        {
            Point[] points = null;

            bool drawNoSecondScaleBar = ScaleBarMode == ScaleBarMode.Single || (ScaleBarMode == ScaleBarMode.Both && SecondaryUnitConverter == null);

            float maxScaleBarLength = Math.Max(scaleBarLength1, scaleBarLength2);

            var posX = CalculatePositionX(0, (int)Viewport.Width, maxWidth);
            var posY = CalculatePositionY(0, (int)Viewport.Height, height);

            float left = posX + stroke * 0.5f * Scale;
            float right = posX + maxWidth - stroke * 0.5f * Scale;
            float center1 = posX + (maxWidth - scaleBarLength1) / 2;
            float center2 = posX + (maxWidth - scaleBarLength2) / 2;
            // Top position is Y in the middle of scale bar line
            float top = posY + (drawNoSecondScaleBar ? height - stroke * 0.5f * Scale : height * 0.5f);

            switch (TextAlignment)
            {
                case Alignment.Center:
                    if (drawNoSecondScaleBar)
                    {
                        points = new Point[6];
                        points[0] = new Point(center1, top - TickLength * Scale);
                        points[1] = new Point(center1, top);
                        points[2] = new Point(center1, top);
                        points[3] = new Point(center1 + maxScaleBarLength, top);
                        points[4] = new Point(center1 + maxScaleBarLength, top);
                        points[5] = new Point(center1 + scaleBarLength1, top - TickLength * Scale);
                    }
                    else
                    {
                        points = new Point[10];
                        points[0] = new Point(Math.Min(center1, center2), top);
                        points[1] = new Point(Math.Min(center1, center2) + maxScaleBarLength, top);
                        points[2] = new Point(center1, top - TickLength * Scale);
                        points[3] = new Point(center1, top);
                        points[4] = new Point(center1 + scaleBarLength1, top - TickLength * Scale);
                        points[5] = new Point(center1 + scaleBarLength1, top);
                        points[6] = new Point(center2, top + TickLength * Scale);
                        points[7] = new Point(center2, top);
                        points[8] = new Point(center2 + scaleBarLength2, top + TickLength * Scale);
                        points[9] = new Point(center2 + scaleBarLength2, top);
                    }
                    break;
                case Alignment.Left:
                    if (drawNoSecondScaleBar)
                    {
                        points = new Point[6];
                        points[0] = new Point(left, top);
                        points[1] = new Point(left + maxScaleBarLength, top);
                        points[2] = new Point(left, top - TickLength * Scale);
                        points[3] = new Point(left, top);
                        points[4] = new Point(left + scaleBarLength1, top - TickLength * Scale);
                        points[5] = new Point(left + scaleBarLength1, top);
                    }
                    else
                    {
                        points = new Point[8];
                        points[0] = new Point(left, top);
                        points[1] = new Point(left + maxScaleBarLength, top);
                        points[2] = new Point(left, top - TickLength * Scale);
                        points[3] = new Point(left, top + TickLength * Scale);
                        points[4] = new Point(left + scaleBarLength1, top - TickLength * Scale);
                        points[5] = new Point(left + scaleBarLength1, top);
                        points[6] = new Point(left + scaleBarLength2, top + TickLength * Scale);
                        points[7] = new Point(left + scaleBarLength2, top);
                    }
                    break;
                case Alignment.Right:
                    if (drawNoSecondScaleBar)
                    {
                        points = new Point[6];
                        points[0] = new Point(right, top);
                        points[1] = new Point(right - maxScaleBarLength, top);
                        points[2] = new Point(right, top - TickLength * Scale);
                        points[3] = new Point(right, top);
                        points[4] = new Point(right - scaleBarLength1, top - TickLength * Scale);
                        points[5] = new Point(right - scaleBarLength1, top);
                    }
                    else
                    {
                        points = new Point[8];
                        points[0] = new Point(right, top);
                        points[1] = new Point(right - maxScaleBarLength, top);
                        points[2] = new Point(right, top - TickLength * Scale);
                        points[3] = new Point(right, top + TickLength * Scale);
                        points[4] = new Point(right - scaleBarLength1, top - TickLength * Scale);
                        points[5] = new Point(right - scaleBarLength1, top);
                        points[6] = new Point(right - scaleBarLength2, top + TickLength * Scale);
                        points[7] = new Point(right - scaleBarLength2, top);
                    }
                    break;
            }

            return points;
        }

        public (float posX1, float posY1, float posX2, float posY2) DrawText(BoundingBox textSize, BoundingBox textSize1, BoundingBox textSize2, float stroke)
        {
            bool drawNoSecondScaleBar = ScaleBarMode == ScaleBarMode.Single || (ScaleBarMode == ScaleBarMode.Both && SecondaryUnitConverter == null);

            float posX = CalculatePositionX(0, (int)Viewport.Width, maxWidth);
            float posY = CalculatePositionY(0, (int)Viewport.Height, height);

            float left = posX + (stroke + TextMargin) * Scale;
            float right1 = posX + maxWidth - (stroke + TextMargin) * Scale - (float)textSize1.Width;
            float right2 = posX + maxWidth - (stroke + TextMargin) * Scale - (float)textSize2.Width;
            float top = posY;
            float bottom = posY + height - (float)textSize2.Height;

            switch (TextAlignment)
            {
                case Alignment.Center:
                    if (drawNoSecondScaleBar)
                    {
                        return (posX + (stroke + TextMargin) * Scale + (MaxWidth - 2.0f * (stroke + TextMargin) * Scale - (float)textSize1.Width) / 2.0f, 
                            top,
                            0, 
                            0);
                    }
                    else
                    {
                        return (posX + (stroke + TextMargin) * Scale + (MaxWidth - 2.0f * (stroke + TextMargin) * Scale - (float)textSize1.Width) / 2.0f,
                                top, 
                                posX + (stroke + TextMargin) * Scale + (MaxWidth - 2.0f * (stroke + TextMargin) * Scale - (float)textSize2.Width) / 2.0f,
                                bottom);
                    }
                case Alignment.Left:
                    if (drawNoSecondScaleBar)
                    {
                        return (left, top, 0, 0);
                    }
                    else
                    {
                        return (left, top, left, bottom);
                    }
                case Alignment.Right:
                    if (drawNoSecondScaleBar)
                    {
                        return (right1, top, 0, 0);
                    }
                    else
                    {
                        return (right1, top, right2, bottom);
                    }
                default:
                    return (0, 0, 0, 0);
            }
        }

        public void ViewChanged(bool majorChange, BoundingBox extent, double resolution)
        {
            // If resolution changes, than we need a redraw
            if (lastResolution != resolution)
            {
                lastResolution = resolution;
            }
        }

        internal void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}