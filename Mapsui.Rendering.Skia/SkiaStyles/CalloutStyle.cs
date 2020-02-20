using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.Widgets;
using SkiaSharp;
using System.Runtime.CompilerServices;

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
        private SKPath _path;
        private SKPoint _center;
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
        private Topten.RichTextKit.Style _styleTitle = new Topten.RichTextKit.Style();
        private Topten.RichTextKit.Style _styleSubtitle = new Topten.RichTextKit.Style();
        private Alignment _titleTextAlignment;
        private Alignment _subtitleTextAlignment;
        private double _spacing;
        private double _maxWidth;
        
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
                    _path = null;
                    OnPropertyChanged();
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
        /// Storage for an own bubble path
        /// </summary>
        public SKPath Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font name to use rendering title
        /// </summary>
        public string TitleFontName
        {
            get => _styleTitle.FontFamily;
            set
            {
                if (_styleTitle.FontFamily != value)
                {
                    _styleTitle.FontFamily = value;
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font size to rendering title
        /// </summary>
        public double TitleFontSize
        {
            get => _styleTitle.FontSize;
            set
            {
                if (_styleTitle.FontSize != value)
                {
                    _styleTitle.FontSize = (float)value;
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font attributes italic to render title
        /// </summary>
        public bool TitleFontItalic
        {
            get => _styleTitle.FontItalic;
            set
            {
                if (_styleTitle.FontItalic != value)
                {
                    _styleTitle.FontItalic = value;
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font attributes bold to render title
        /// </summary>
        public bool TitleFontBold
        {
            get => _styleTitle.FontWeight > 400;
            set
            {
                _styleTitle.FontWeight = (value ? 700 : 400);
                _path = null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Font color to render title
        /// </summary>
        public Color TitleFontColor
        {
            get => _styleTitle.TextColor.ToMapsui();
            set
            {
                if (_styleTitle.TextColor.ToMapsui() != value)
                {
                    _styleTitle.TextColor = value.ToSkia(1f);
                    _path = null;
                    OnPropertyChanged();
                }
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font name to use rendering subtitle
        /// </summary>
        public string SubtitleFontName
        {
            get => _styleTitle.FontFamily;
            set
            {
                if (_styleTitle.FontFamily != value)
                {
                    _styleTitle.FontFamily = value;
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font size to rendering subtitle
        /// </summary>
        public double SubtitleFontSize
        {
            get => _styleSubtitle.FontSize;
            set
            {
                if (_styleSubtitle.FontSize != value)
                {
                    _styleSubtitle.FontSize = (float)value;
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font attributes italic to render subtitle
        /// </summary>
        public bool SubtitleFontItalic
        {
            get => _styleSubtitle.FontItalic;
            set
            {
                if (_styleSubtitle.FontItalic != value)
                {
                    _styleSubtitle.FontItalic = value;
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font attributes bold to render subtitle
        /// </summary>
        public bool SubtitleFontBold
        {
            get => _styleSubtitle.FontWeight > 400;
            set
            {
                _styleSubtitle.FontWeight = (value ? 700 : 400);
                _path = null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Font color to render subtitle
        /// </summary>
        public Color SubtitleFontColor
        {
            get => _styleSubtitle.TextColor.ToMapsui();
            set
            {
                if (_styleSubtitle.TextColor.ToMapsui() != value)
                {
                    _styleSubtitle.TextColor = value.ToSkia(1f);
                    _path = null;
                    OnPropertyChanged();
                }
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
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
                    _path = null;
                    OnPropertyChanged();
                }
            }
        }

        public int InternalContent { get; set; } = -1;
        public SKPoint Center { get => _center; set => _center = value; }

        /// <summary>
        /// Something changed, so create new image
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (_content < 0 && _type == CalloutType.Custom)
                return;

            // Create content of this Callout
            if (propertyName.Equals(nameof(Title))
                || propertyName.Equals(nameof(TitleFontName))
                || propertyName.Equals(nameof(TitleFontSize))
                || propertyName.Equals(nameof(TitleFontItalic))
                || propertyName.Equals(nameof(TitleFontBold))
                || propertyName.Equals(nameof(TitleFontColor))
                || propertyName.Equals(nameof(TitleTextAlignment))
                || propertyName.Equals(nameof(Subtitle))
                || propertyName.Equals(nameof(SubtitleFontName))
                || propertyName.Equals(nameof(SubtitleFontSize))
                || propertyName.Equals(nameof(SubtitleFontItalic))
                || propertyName.Equals(nameof(SubtitleFontBold))
                || propertyName.Equals(nameof(SubtitleFontColor))
                || propertyName.Equals(nameof(SubtitleTextAlignment))
                || propertyName.Equals(nameof(Spacing))
                || propertyName.Equals(nameof(MaxWidth)))
            {
                CalloutStyleRenderer.UpdateContent(this);
            }

            CalloutStyleRenderer.RenderCallout(this);
        }
    }
}
