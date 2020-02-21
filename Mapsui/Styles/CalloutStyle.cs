using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering.Skia
{
    /// <summary>
    /// Type of CalloutStyle
    /// </summary>
    public enum CalloutType
    {
        /// <summary>
        /// Only one line is shown
        /// </summary>
        Single,
        /// <summary>
        /// Header and detail is shown
        /// </summary>
        Detail,
        /// <summary>
        /// Content is custom, the bitmap given in Content is shown
        /// </summary>
        Custom,
    }

    /// <summary>
    /// Determins, where the pointer is
    /// </summary>
    public enum ArrowAlignment
    {
        /// <summary>
        /// Callout arrow is at bottom side of bubble
        /// </summary>
        Bottom,
        /// <summary>
        /// Callout arrow is at left side of bubble
        /// </summary>
        Left,
        /// <summary>
        /// Callout arrow is at top side of bubble
        /// </summary>
        Top,
        /// <summary>
        /// Callout arrow is at right side of bubble
        /// </summary>
        Right,
    }

    public class CalloutStyle : SymbolStyle
    {
        private CalloutType _type = CalloutType.Single;
        private ArrowAlignment _arrowAlignment = ArrowAlignment.Bottom;
        private float _arrowWidth = 8f;
        private float _arrowHeight = 8f;
        private float _arrowPosition = 0.5f;
        private float _rectRadius = 4f;
        private float _shadowWidth = 2f;
        private BoundingBox _padding = new BoundingBox(3f, 3f, 3f, 3f);
        private Color _color = Color.Black;
        private Color _backgroundColor = Color.White;
        private float _strokeWidth = 1f;
        private int _content = -1;
        private Point _offset = new Point(0, 0);
        private double _rotation = 0;
        private string _title;
        private string _subtitle;
        private Alignment _titleTextAlignment;
        private Alignment _subtitleTextAlignment;
        private double _spacing;
        private double _maxWidth;
        private Color _titleFontColor;
        private Color _subtitleFontColor;

        public new static double DefaultWidth { get; set; } = 100;
        public new static double DefaultHeight { get; set; } = 30;

        public CalloutStyle()
        {
        }

        /// <summary>
        /// Type of Callout
        /// </summary>
        /// <remarks>
        /// Could be single, detail or custom. The last is a bitmap id for an owner drawn image.
        /// </remarks>
        public CalloutType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Offset position in pixels of Callout
        /// </summary>
        public Point Offset
        {
            get => _offset;
            set
            {
                if (!_offset.Equals(value))
                {
                    _offset = value;
                    SymbolOffset = new Offset(_offset.X, _offset.Y);
                }
            }
        }

        /// <summary>
        /// BoundingBox relative to offset point
        /// </summary>
        public BoundingBox BoundingBox = new BoundingBox();

        /// <summary>
        /// Gets or sets the rotation of the Callout in degrees (clockwise is positive)
        /// </summary>
        public double Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    SymbolRotation = _rotation;
                }
            }
        }

        /// <summary>
        /// Anchor position of Callout
        /// </summary>
        public ArrowAlignment ArrowAlignment
        {
            get => _arrowAlignment;
            set
            {
                if (value != _arrowAlignment)
                {
                    _arrowAlignment = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Width of opening of anchor of Callout
        /// </summary>
        public float ArrowWidth
        {
            get => _arrowWidth;
            set
            {
                if (value != _arrowWidth)
                {
                    _arrowWidth = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Height of anchor of Callout
        /// </summary>
        public float ArrowHeight
        {
            get => _arrowHeight;
            set
            {
                if (value != _arrowHeight)
                {
                    _arrowHeight = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Relative position of anchor of Callout on the side given by AnchorType
        /// </summary>
        public float ArrowPosition
        {
            get => _arrowPosition;
            set
            {
                if (value != _arrowPosition)
                {
                    _arrowPosition = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Color of stroke around Callout
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {
                if (value != _color)
                {
                    _color = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// BackgroundColor of Callout
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (value != _backgroundColor)
                {
                    _backgroundColor = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Stroke width of frame around Callout
        /// </summary>
        public float StrokeWidth
        {
            get => _strokeWidth;
            set
            {
                if (value != _strokeWidth)
                {
                    _strokeWidth = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Radius of rounded corners of Callout
        /// </summary>
        public float RectRadius
        {
            get => _rectRadius;
            set
            {
                if (value != _rectRadius)
                {
                    _rectRadius = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Padding around content of Callout
        /// </summary>
        public BoundingBox Padding
        {
            get => _padding;
            set
            {
                if (value != _padding)
                {
                    _padding = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Width of shadow around Callout
        /// </summary>
        public float ShadowWidth
        {
            get => _shadowWidth;
            set
            {
                if (value != _shadowWidth)
                {
                    _shadowWidth = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Content of Callout
        /// </summary>
        /// <remarks>
        /// Is a BitmapId of a save image
        /// </remarks>
        public int Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Content of Callout title label
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Font color to render title
        /// </summary>
        public Color TitleFontColor 
        {
            get { return _titleFontColor; }
            set
            {
                _titleFontColor = value;
                Invalidated = true;
            }
        }

        /// <summary>
        /// Text alignment of title
        /// </summary>
        public Alignment TitleTextAlignment
        {
            get => _titleTextAlignment;
            set
            {
                if (_titleTextAlignment != value)
                {
                    _titleTextAlignment = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Content of Callout subtitle label
        /// </summary>
        public string Subtitle
        {
            get => _subtitle;
            set
            {
                if (_subtitle != value)
                {
                    _subtitle = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Font color to render subtitle
        /// </summary>
        public Color SubtitleFontColor
        {
            get { return _subtitleFontColor; }
            set
            {
                _subtitleFontColor = value;
                Invalidated = true;
            }
        }

        /// <summary>
        /// Text alignment of subtitle
        /// </summary>
        public Alignment SubtitleTextAlignment
        {
            get => _subtitleTextAlignment;
            set
            {
                if (_subtitleTextAlignment != value)
                {
                    _subtitleTextAlignment = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// Space between Title and Subtitel of Callout
        /// </summary>
        public double Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    Invalidated = true;
                }
            }
        }

        /// <summary>
        /// MaxWidth for Title and Subtitel of Callout
        /// </summary>
        public double MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (_maxWidth != value)
                {
                    _maxWidth = value;
                    Invalidated = true;
                }
            }
        }

        public int InternalContent { get; set; } = -1;

        private Font _titleFont = new Font();
        private Font _subtitleFont = new Font();

        public Font TitleFont
        {
            get
            {
                return _titleFont;
            }
            set 
            {
                _titleFont = value;
                Invalidated = true;
            }
        }

        public Font SubtitleFont
        {
            get
            {
                return _subtitleFont;
            }
            set
            {
                _subtitleFont = value;
                Invalidated = true;
            }
        }

        public bool Invalidated { get; set; }
    }
}
