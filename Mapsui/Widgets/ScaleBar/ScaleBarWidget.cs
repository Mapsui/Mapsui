using Mapsui.Geometries;
using Mapsui.Styles;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mapsui.Logging;

namespace Mapsui.Widgets.ScaleBar
{
    /// <summary>
    /// A ScaleBarWidget displays the ratio of a distance on the map to the corresponding distance on the ground.
    /// It uses always the center of a given Viewport to calc this ratio.
    ///
    /// Usage
    /// To show a ScaleBarWidget, add a instance of the ScaleBarWidget to Map.Widgets by
    /// 
    ///   map.Widgets.Add(new ScaleBarWidget(map));
    ///   
    /// Customize
    /// ScaleBarMode: Determins, how much scalebars are shown. Could be Single or Both.
    /// SecondaryUnitConverter: First UnitConverter for upper scalebar. There are UnitConverters for metric, imperial and nautical units.
    /// SecondaryUnitConverter = NauticalUnitConverter.Instance });
    /// MaxWidth: Maximal width of the scalebar. Real width could be smaller.
    /// HorizontalAlignment: Where the ScaleBarWidget is shown. Could be Left, Right, Center or Position.
    /// VerticalAlignment: Where the ScaleBarWidget is shown. Could be Top, Bottom, Center or Position.
    /// PositionX: If HorizontalAlignment is Position, this value determins the distance to the left
    /// PositionY: If VerticalAlignment is Position, this value determins the distance to the top
    /// TextColor: Color for text and lines
    /// Halo: Color used around text and lines, so the scalebar is better visible
    /// TextAlignment: Alignment of scalebar text to the lines. Could be Left, Right or Center
    /// TextMargin: Space between text and lines of scalebar
    /// Font: Font which is used to draw text
    /// TickLength: Length of the ticks at scalebar
    /// </summary>
    public class ScaleBarWidget : Widget, INotifyPropertyChanged
    {
        private readonly Map _map;

        ///
        /// Default position of the scale bar.
        ///
        private static readonly HorizontalAlignment DefaultScaleBarHorizontalAlignment = HorizontalAlignment.Left;
        private static readonly VerticalAlignment DefaultScaleBarVerticalAlignment = VerticalAlignment.Bottom;
        private static readonly Alignment DefaultScaleBarAlignment = Alignment.Left;
        private static readonly ScaleBarMode DefaultScaleBarMode = ScaleBarMode.Single;
        private static readonly Font DefaultFont = new Font { FontFamily = "Arial", Size = 10 };

        public ScaleBarWidget(Map map)
        {
            _map = map;

            HorizontalAlignment = DefaultScaleBarHorizontalAlignment;
            VerticalAlignment = DefaultScaleBarVerticalAlignment;

            _maxWidth = 100;
            _height = 100;
            _textAlignment = DefaultScaleBarAlignment;
            _scaleBarMode = DefaultScaleBarMode;

            _unitConverter = MetricUnitConverter.Instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        float _maxWidth;

        /// <summary>
        /// Maximum usable width for scalebar. The real used width could be less, because we 
        /// want only integers as text.
        /// </summary>
        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_maxWidth == value)
                    return;

                _maxWidth = value;
                OnPropertyChanged();
            }
        }

        float _height;

        /// <summary>
        /// Real height of scalebar. Depends on number of unit converters and text size.
        /// Is calculated by renderer.
        /// </summary>
        public float Height
        {
            get => _height;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_height == value)
                    return;

                _height = value;
                OnPropertyChanged();
            }
        }

        Color _textColor = new Color(0, 0, 0);

        /// <summary>
        /// Foreground color of scalebar and text
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor == value)
                    return;
                _textColor = value;
                OnPropertyChanged();
            }
        }

        Color _haloColor = new Color(255, 255, 255);

        /// <summary>
        /// Halo color of scalebar and text, so that it is better visible
        /// </summary>
        public Color Halo
        {
            get => _haloColor;
            set
            {
                if (_haloColor == value)
                    return;
                _haloColor = value;
                OnPropertyChanged();
            }
        }

        public float Scale { get; } = 1;

        /// <summary>
        /// Stroke width for lines
        /// </summary>
        public float StrokeWidth { get; set; } = 2;

        /// <summary>
        /// Stroke width for halo of lines
        /// </summary>
        public float StrokeWidthHalo { get; set; } = 4;

        /// <summary>
        /// Length of the ticks
        /// </summary>
        public float TickLength { get; set; } = 3;

        Alignment _textAlignment;

        /// <summary>
        /// Alignment of text of scalebar
        /// </summary>
        public Alignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                if (_textAlignment == value)
                    return;

                _textAlignment = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Margin between end of tick and text
        /// </summary>
        public float TextMargin { get; } = 1;

        private Font _font = DefaultFont;

        /// <summary>
        /// Font to use for drawing text
        /// </summary>
        public Font Font
        {
            get => _font ?? DefaultFont;
            set
            {
                if (_font == value)
                    return;

                _font = value;
                OnPropertyChanged();
            }
        }

        private IUnitConverter _unitConverter;

        /// <summary>
        /// Normal unit converter for upper text. Default is MetricUnitConverter.
        /// </summary>
        public IUnitConverter UnitConverter
        {
            get => _unitConverter;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException($"{nameof(UnitConverter)} must not be null");
                }
                if (_unitConverter == value)
                {
                    return;
                }

                _unitConverter = value;
                OnPropertyChanged();
            }
        }

        private IUnitConverter _secondaryUnitConverter;
        
        /// <summary>
        /// Secondary unit converter for lower text if ScaleBarMode is Both. Default is ImperialUnitConverter.
        /// </summary>
        public IUnitConverter SecondaryUnitConverter
        {
            get => _secondaryUnitConverter;
            set
            {
                if (_secondaryUnitConverter == value)
                {
                    return;
                }

                _secondaryUnitConverter = value;
                OnPropertyChanged();
            }
        }

        private ScaleBarMode _scaleBarMode;

        /// <summary>
        /// ScaleBarMode of scalebar. Could be Single to show only one or Both for showing two units.
        /// </summary>
        public ScaleBarMode ScaleBarMode
        {
            get => _scaleBarMode;
            set
            {
                if (_scaleBarMode == value)
                {
                    return;
                }

                _scaleBarMode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Draw a rectangle around the scale bar for testing
        /// </summary>
        public bool ShowEnvelop { get; set; }

        /// <summary>
        /// Calculates the length and text for both scalebars
        /// </summary>
        /// <returns>
        /// Length of upper scalebar
        /// Text of upper scalebar
        /// Length of lower scalebar
        /// Text of lower scalebar
        /// </returns>
        public (float scaleBarLength1, string scaleBarText1, float scaleBarLength2, string scaleBarText2) 
            GetScaleBarLengthAndText(IReadOnlyViewport viewport)
        {
            if (_map == null) return (0, null, 0, null);

            float length1;
            string text1;
            
            (length1, text1) = CalculateScaleBarLengthAndValue(_map, viewport, MaxWidth, UnitConverter);

            float length2;
            string text2;

            if (SecondaryUnitConverter != null)
                (length2, text2) = CalculateScaleBarLengthAndValue(_map, viewport, MaxWidth, SecondaryUnitConverter);
            else
                (length2, text2) = (0, null);

            return (length1, text1, length2, text2);
        }

        /// <summary>
        /// Get pairs of points, which determin start and stop of the lines used to draw the scalebar
        /// </summary>
        /// <param name="viewport">The viewport of the map</param>
        /// <param name="scaleBarLength1">Length of upper scalebar</param>
        /// <param name="scaleBarLength2">Length of lower scalebar</param>
        /// <param name="stroke">Width of line</param>
        /// <returns>Array with pairs of Points. First is always the start point, the second is the end point.</returns>
        public Point[] GetScaleBarLinePositions(IReadOnlyViewport viewport, float scaleBarLength1, float scaleBarLength2, float stroke)
        {
            Point[] points = null;

            bool drawNoSecondScaleBar = ScaleBarMode == ScaleBarMode.Single || ScaleBarMode == ScaleBarMode.Both && SecondaryUnitConverter == null;

            float maxScaleBarLength = Math.Max(scaleBarLength1, scaleBarLength2);

            var posX = CalculatePositionX(0, (int)viewport.Width, _maxWidth);
            var posY = CalculatePositionY(0, (int)viewport.Height, _height);

            float left = posX + stroke * 0.5f * Scale;
            float right = posX + _maxWidth - stroke * 0.5f * Scale;
            float center1 = posX + (_maxWidth - scaleBarLength1) / 2;
            float center2 = posX + (_maxWidth - scaleBarLength2) / 2;
            // Top position is Y in the middle of scale bar line
            float top = posY + (drawNoSecondScaleBar ? _height - stroke * 0.5f * Scale : _height * 0.5f);

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

        /// <summary>
        /// Calculates the top-left-position of upper and lower text
        /// </summary>
        /// <param name="viewport">The viewport</param>
        /// <param name="textSize">Default textsize for the string "9999 m"</param>
        /// <param name="textSize1">Size of upper text of scalebar</param>
        /// <param name="textSize2">Size of lower text of scalebar</param>
        /// <param name="stroke">Width of line</param>
        /// <returns>
        /// posX1 as left position of upper scalebar text
        /// posY1 as top position of upper scalebar text
        /// posX2 as left position of lower scalebar text
        /// posY2 as top position of lower scalebar text
        /// </returns>
        public (float posX1, float posY1, float posX2, float posY2) GetScaleBarTextPositions(IReadOnlyViewport viewport, 
            BoundingBox textSize, BoundingBox textSize1, BoundingBox textSize2, float stroke)
        {
            bool drawNoSecondScaleBar = ScaleBarMode == ScaleBarMode.Single || (ScaleBarMode == ScaleBarMode.Both && SecondaryUnitConverter == null);

            float posX = CalculatePositionX(0, (int)viewport.Width, _maxWidth);
            float posY = CalculatePositionY(0, (int)viewport.Height, _height);

            float left = posX + (stroke + TextMargin) * Scale;
            float right1 = posX + _maxWidth - (stroke + TextMargin) * Scale - (float)textSize1.Width;
            float right2 = posX + _maxWidth - (stroke + TextMargin) * Scale - (float)textSize2.Width;
            float top = posY;
            float bottom = posY + _height - (float)textSize2.Height;

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

        public override bool HandleWidgetTouched(INavigator navigator, Point position)
        {
            return false;
        }

        public bool CanTransform()
        {
            if (_map?.CRS == null) 
            {
                Logger.Log(LogLevel.Warning, $"ScaleBarWidget can not draw because the {nameof(Map)}.{nameof(Map.CRS)} is not set");
                return false;
            }

            if (_map.Transformation == null)
            {
                Logger.Log(LogLevel.Warning, $"ScaleBarWidget can not draw because the {nameof(Map)}.{nameof(Map.Transformation)} is not set");
                return false;
            }

            if (_map.Transformation.IsProjectionSupported(_map.CRS, "EPSG:4326") != true)
            {
                Logger.Log(LogLevel.Warning, $"ScaleBarWidget can not draw because the projection between {_map.CRS} and EPSG:4326 is not supported");
                return false;
            }
            return true;
        }


        internal void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// Calculates the required length and value of a scalebar
        ///
        /// @param viewport the Viewport to calculate for
        /// @param width of the scale bar in pixel to calculate for
        /// @param unitConverter the DistanceUnitConverter to calculate for
        /// @return scaleBarLength and scaleBarText
        private static (float scaleBarLength, string scaleBarText) CalculateScaleBarLengthAndValue(
            Map map, IReadOnlyViewport viewport, float width, IUnitConverter unitConverter)
        {
            // We have to calc the angle difference to the equator (angle = 0), 
            // because EPSG:3857 is only there 1 m. At othere angles, we
            // should calculate the correct length.
            var position = (Point)map.Transformation.Transform(map.CRS, "EPSG:4326", ((Point)viewport.Center).Clone()); // clone or else you will transform the orginal viewport center

            // Calc ground resolution in meters per pixel of viewport for this latitude
            double groundResolution = viewport.Resolution * Math.Cos(position.Y / 180.0 * Math.PI);

            // Convert in units of UnitConverter
            groundResolution = groundResolution / unitConverter.MeterRatio;

            var scaleBarValues = unitConverter.ScaleBarValues;

            float scaleBarLength = 0;
            int scaleBarValue = 0;

            foreach (int value in scaleBarValues)
            {
                scaleBarValue = value;
                scaleBarLength = (float)(scaleBarValue / groundResolution);
                if (scaleBarLength < width - 10)
                {
                    break;
                }
            }

            var scaleBarText = unitConverter.GetScaleText(scaleBarValue);

            return (scaleBarLength, scaleBarText);
        }
    }
}