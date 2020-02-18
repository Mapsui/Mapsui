using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Topten.RichTextKit;

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
        private int _internalContent = -1;
        private string _title;
        private Topten.RichTextKit.Style _styleTitle = new Topten.RichTextKit.Style();
        private TextBlock _textBlockTitle = new TextBlock();
        private string _subtitle;
        private Topten.RichTextKit.Style _styleSubtitle = new Topten.RichTextKit.Style();
        private TextBlock _textBlockSubtitle = new TextBlock();
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
        public TextAlignment TitleTextAlignment
        {
            get => _textBlockTitle.Alignment;
            set
            {
                if (_textBlockTitle.Alignment != value)
                {
                    _textBlockTitle.Alignment = value;
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
        public TextAlignment SubtitleTextAlignment
        {
            get => _textBlockSubtitle.Alignment;
            set
            {
                if (_textBlockSubtitle.Alignment != value)
                {
                    _textBlockSubtitle.Alignment = value;
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
                UpdateContent();
            }

            if (_content < 0)
                return;

            // Get size of content
            var bitmapInfo = BitmapHelper.LoadBitmap(BitmapRegistry.Instance.Get(_content));

            double contentWidth = bitmapInfo.Width;
            double contentHeight = bitmapInfo.Height;

            (var width, var height) = CalcSize(contentWidth, contentHeight);

            // Create a canvas for drawing
            var info = new SKImageInfo((int)width, (int)height);
            using (var surface = SKSurface.Create(info))
            {
                var canvas = surface.Canvas;

                // Is there a prerendered path?
                if (_path == null)
                {
                    // No, than create a new path
                    (_path, _center) = CreateCalloutPath(contentWidth, contentHeight);
                    // Now move SymbolOffset to the position of the arrow
                    SymbolOffset = new Offset(Offset.X + (width * 0.5 - _center.X), Offset.Y - (height * 0.5 - _center.Y));
                }

                // Draw path for bubble
                DrawCallout(canvas);

                // Draw content
                DrawContent(canvas, bitmapInfo);

                // Create image from canvas
                var image = surface.Snapshot();
                var data = image.Encode(SKEncodedImageFormat.Png, 100);

                // Register 
                //if (BitmapId >= 0)
                //    BitmapRegistry.Instance.Set(BitmapId, data.AsStream(true));
                //else
                    BitmapId = BitmapRegistry.Instance.Register(data.AsStream(true));
            }
        }

        /// <summary>
        /// Calc the size which is needed for the canvas
        /// </summary>
        /// <returns></returns>
        private (double, double) CalcSize(double contentWidth, double contentHeight)
        {
            var strokeWidth = _strokeWidth < 1 ? 1 : _strokeWidth;
            // Add padding around the content
            var paddingLeft = _padding.Left < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Left;
            var paddingTop = _padding.Top < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Top;
            var paddingRight = _padding.Right < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Right;
            var paddingBottom = _padding.Bottom < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Bottom;
            var width = contentWidth + paddingLeft + paddingRight + 1;
            var height = contentHeight + paddingTop + paddingBottom + 1;

            // Add length of arrow
            switch (ArrowAlignment)
            {
                case ArrowAlignment.Bottom:
                case ArrowAlignment.Top:
                    height += ArrowHeight;
                    break;
                case ArrowAlignment.Left:
                case ArrowAlignment.Right:
                    width += ArrowHeight;
                    break;
            }

            // Add StrokeWidth to all sides
            width += strokeWidth * 2;
            height += strokeWidth * 2;

            // Add shadow to all sides
            width += _shadowWidth * 2;
            height += _shadowWidth * 2;

            return (width, height);
        }

        private void DrawCallout(SKCanvas canvas)
        {
            var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _shadowWidth) };
            var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ToSkia(_backgroundColor) };
            var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = ToSkia(_color), StrokeWidth = _strokeWidth };

            canvas.Clear(SKColors.Transparent);
            canvas.DrawPath(_path, shadow);
            canvas.DrawPath(_path, fill);
            canvas.DrawPath(_path, stroke);
        }

        /// <summary>
        /// Update content for single and detail
        /// </summary>
        private void UpdateContent()
        {
            if (Type == CalloutType.Custom)
                return;

            _textBlockTitle.Clear();
            _textBlockSubtitle.Clear();

            switch (Type)
            {
                case CalloutType.Detail:
                    _textBlockSubtitle.Alignment = SubtitleTextAlignment;
                    _textBlockSubtitle.AddText(Subtitle, _styleSubtitle);
                    goto case CalloutType.Single;
                case CalloutType.Single:
                    _textBlockTitle.Alignment = TitleTextAlignment;
                    _textBlockTitle.AddText(Title, _styleTitle);
                    CreateContent();
                    break;
            }
        }

        /// <summary>
        /// Create content BitmapId from given TextBlock
        /// </summary>
        private void CreateContent()
        {
            _textBlockTitle.MaxWidth = _textBlockSubtitle.MaxWidth = (float)MaxWidth;
            // Layout TextBlocks
            _textBlockTitle.Layout();
            _textBlockSubtitle.Layout();
            // Get sizes
            var width = Math.Max(_textBlockTitle.MeasuredWidth, _textBlockSubtitle.MeasuredWidth);
            var height = _textBlockTitle.MeasuredHeight + (Type == CalloutType.Detail ? _textBlockSubtitle.MeasuredHeight + Spacing : 0);
            // Now we have the correct width, so make a new layout cycle for text alignment
            _textBlockTitle.MaxWidth = _textBlockSubtitle.MaxWidth = width;
            _textBlockTitle.Layout();
            _textBlockSubtitle.Layout();
            // Create bitmap from TextBlock
            var info = new SKImageInfo((int)width, (int)height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(info))
            {
                var canvas = surface.Canvas;
                var memStream = new MemoryStream();

                canvas.Clear(SKColors.Transparent);
                // surface.Canvas.Scale(DeviceDpi / 96.0f);
                _textBlockTitle.Paint(canvas, new TextPaintOptions() { IsAntialias = true });
                _textBlockSubtitle.Paint(canvas, new SKPoint(0, _textBlockTitle.MeasuredHeight + (float)Spacing), new TextPaintOptions() { IsAntialias = true });
                // Create image from canvas
                var image = surface.Snapshot();
                var data = image.Encode(SKEncodedImageFormat.Png, 100);
                if (_internalContent >= 0)
                {
                    BitmapRegistry.Instance.Set(_internalContent, data.AsStream(true));
                }
                else
                {
                    _internalContent = BitmapRegistry.Instance.Register(data.AsStream(true));
                }
                _content = _internalContent;
            }
        }

        private void DrawContent(SKCanvas canvas, BitmapInfo bitmapInfo)
        { 
            // Draw content
            if (_content >= 0)
            {
                var strokeWidth = _strokeWidth < 1 ? 1 : _strokeWidth;
                var offsetX = _shadowWidth + strokeWidth * 2 + (_padding.Left < _rectRadius * 0.5 ? _rectRadius * 0.5f : (float)_padding.Left);
                var offsetY = _shadowWidth + strokeWidth * 2 + (_padding.Top < _rectRadius * 0.5 ? _rectRadius * 0.5f : (float)_padding.Top);

                switch (ArrowAlignment)
                {
                    case ArrowAlignment.Left:
                        offsetX += ArrowHeight;
                        break;
                    case ArrowAlignment.Top:
                        offsetY += ArrowHeight;
                        break;
                }

                var offset = new SKPoint(offsetX, offsetY);

                switch (bitmapInfo.Type)
                {
                    case BitmapType.Bitmap:
                        canvas.DrawImage(bitmapInfo.Bitmap, offset);
                        break;
                    case BitmapType.Sprite:
                        throw new System.Exception();
                    case BitmapType.Svg:
                        canvas.DrawPicture(bitmapInfo.Svg.Picture, offset);
                        break;
                }
            }
        }

        /// <summary>
        /// Update path
        /// </summary>
        private (SKPath, SKPoint) CreateCalloutPath(double contentWidth, double contentHeight)
        {
            var strokeWidth = _strokeWidth < 1 ? 1 : _strokeWidth;
            var paddingLeft = _padding.Left < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Left;
            var paddingTop = _padding.Top < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Top;
            var paddingRight = _padding.Right < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Right;
            var paddingBottom = _padding.Bottom < _rectRadius * 0.5 ? _rectRadius * 0.5 : _padding.Bottom;
            var width = (float)contentWidth + (float)paddingLeft + (float)paddingRight;
            var height = (float)contentHeight + (float)paddingTop + (float)paddingBottom;
            var halfWidth = width * _arrowPosition;
            var halfHeight = height * _arrowPosition;
            var bottom = (float)height + _shadowWidth + strokeWidth * 2;
            var left = _shadowWidth + strokeWidth;
            var top = _shadowWidth + strokeWidth;
            var right = (float)width + _shadowWidth + strokeWidth * 2;
            var start = new SKPoint();
            var center = new SKPoint();
            var end = new SKPoint();

            // Check, if we are to near at corners
            if (halfWidth - _arrowWidth * 0.5f - left < _rectRadius)
                halfWidth = _arrowWidth * 0.5f + left + _rectRadius;
            else if (halfWidth + _arrowWidth * 0.5f > width - _rectRadius)
                halfWidth = width - _arrowWidth * 0.5f - _rectRadius;
            if (halfHeight - _arrowWidth * 0.5f - top < _rectRadius)
                halfHeight = _arrowWidth * 0.5f + top + _rectRadius;
            else if (halfHeight + _arrowWidth * 0.5f > height - _rectRadius)
                halfHeight = height - _arrowWidth * 0.5f - _rectRadius;

            switch (_arrowAlignment)
            {
                case ArrowAlignment.Bottom:
                    start = new SKPoint(halfWidth + _arrowWidth * 0.5f, bottom);
                    center = new SKPoint(halfWidth, bottom + _arrowHeight);
                    end = new SKPoint(halfWidth - _arrowWidth * 0.5f, bottom);
                    break;
                case ArrowAlignment.Top:
                    top += _arrowHeight;
                    bottom += _arrowHeight;
                    start = new SKPoint(halfWidth - _arrowWidth * 0.5f, top);
                    center = new SKPoint(halfWidth, top - _arrowHeight);
                    end = new SKPoint(halfWidth + _arrowWidth * 0.5f, top);
                    break;
                case ArrowAlignment.Left:
                    left += _arrowHeight;
                    right += _arrowHeight;
                    start = new SKPoint(left, halfHeight + _arrowWidth * 0.5f);
                    center = new SKPoint(left - _arrowHeight, halfHeight);
                    end = new SKPoint(left, halfHeight - _arrowWidth * 0.5f);
                    break;
                case ArrowAlignment.Right:
                    start = new SKPoint(right, halfHeight - _arrowWidth * 0.5f);
                    center = new SKPoint(right + _arrowHeight, halfHeight);
                    end = new SKPoint(right, halfHeight + _arrowWidth * 0.5f);
                    break;
            }

            // Create path
            var path = new SKPath();

            // Move to start point at left/top
            path.MoveTo(left + _rectRadius, top);

            // Top horizontal line
            if (ArrowAlignment == ArrowAlignment.Top)
                DrawArrow(path, start, center, end);

            // Top right arc
            path.ArcTo(new SKRect(right - _rectRadius, top, right, top + _rectRadius), 270, 90, false);

            // Right vertical line
            if (ArrowAlignment == ArrowAlignment.Right)
                DrawArrow(path, start, center, end);

            // Bottom right arc
            path.ArcTo(new SKRect(right - _rectRadius, bottom - _rectRadius, right, bottom), 0, 90, false);

            // Bottom horizontal line
            if (ArrowAlignment == ArrowAlignment.Bottom)
                DrawArrow(path, start, center, end);

            // Bottom left arc
            path.ArcTo(new SKRect(left, bottom - _rectRadius, left + _rectRadius, bottom), 90, 90, false);

            // Left vertical line
            if (ArrowAlignment == ArrowAlignment.Left)
                DrawArrow(path, start, center, end);

            // Top left arc
            path.ArcTo(new SKRect(left, top, left + _rectRadius, top + _rectRadius), 180, 90, false);

            path.Close();

            return (path, center);
        }

        /// <summary>
        /// Draw arrow to path
        /// </summary>
        /// <param name="start">Start of arrow at bubble</param>
        /// <param name="center">Center of arrow</param>
        /// <param name="end">End of arrow at bubble</param>
        private static void DrawArrow(SKPath path, SKPoint start, SKPoint center, SKPoint end)
        {
            path.LineTo(start);
            path.LineTo(center);
            path.LineTo(end);
        }

        /// <summary>
        /// Convert Mapsui color to Skia color
        /// </summary>
        /// <param name="color">Color in Mapsui format</param>
        /// <returns>Color in Skia format</returns>
        public SKColor ToSkia(Color color)
        {
            if (color == null) return new SKColor(128, 128, 128, 0);
            return new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
        }
    }
}
