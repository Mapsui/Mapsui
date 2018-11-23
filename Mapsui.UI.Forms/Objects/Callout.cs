using Mapsui.UI.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Mapsui.UI.Objects
{
    /// <summary>
    /// Type of Callout
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
        /// Content is custom, ContentView in Content is shown
        /// </summary>
        Custom,
    }

    /// <summary>
    /// Determins, where the pointer is
    /// </summary>
    public enum ArrowAlignment
    {
        Bottom,
        Left,
        Top,
        Right,
    }

    public class Callout : ContentView
    {
        private MapControl _mapControl;
        private SKCanvasView _background;
        private Grid _grid;
        private Label _title;
        private Label _subtitle;
        private ContentView _content;
        private Button _close;
        private SKPath _path;
        private Point _offset;
        private float _shadowWidth = 2;

        public event EventHandler<EventArgs> CalloutClosed;
        public event EventHandler<EventArgs> CalloutClicked;

        public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(CalloutType), typeof(MapView), default(CalloutType));
        public static readonly BindableProperty AnchorProperty = BindableProperty.Create(nameof(Anchor), typeof(Position), typeof(MapView), default(Position));
        public static readonly BindableProperty ArrowAlignmentProperty = BindableProperty.Create(nameof(ArrowAlignment), typeof(ArrowAlignment), typeof(MapView), default(ArrowAlignment), defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty ArrowWidthProperty = BindableProperty.Create(nameof(ArrowWidth), typeof(float), typeof(MapView), 12f);
        public static readonly BindableProperty ArrowHeightProperty = BindableProperty.Create(nameof(ArrowHeight), typeof(float), typeof(MapView), 16f);
        public static readonly BindableProperty ArrowPositionProperty = BindableProperty.Create(nameof(ArrowPosition), typeof(float), typeof(MapView), 0.5f);
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(MapView), Color.White);
        public static readonly new BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(MapView), Color.White);
        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(MapView), default(float));
        public static readonly BindableProperty RectRadiusProperty = BindableProperty.Create(nameof(RectRadius), typeof(float), typeof(MapView), default(float));
        public static readonly new BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(MapView), new Thickness(6));
        public static readonly new BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(MapView), true);
        public static readonly BindableProperty IsCloseVisibleProperty = BindableProperty.Create(nameof(IsCloseVisible), typeof(bool), typeof(MapView), true);
        public static readonly BindableProperty IsClosableByClickProperty = BindableProperty.Create(nameof(IsClosableByClick), typeof(bool), typeof(MapView), true);
        public static readonly new BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(ContentView), typeof(MapView), null);
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(MapView), default(string));
        public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(MapView), default(string));

        public Callout(MapControl mapControl)
        {
            _mapControl = mapControl ?? throw new ArgumentNullException("MapControl shouldn't be null");

            // We want any drawing outside of the info window
            IsClippedToBounds = true;

            // Defaults
            base.BackgroundColor = Color.Transparent;
            base.Padding = new Thickness(0);

            _background = new SKCanvasView
            {
                BackgroundColor = Color.Transparent,
            };

            _background.PaintSurface += HandlePaintSurface;

            AbsoluteLayout.SetLayoutBounds(_background, new Rectangle(0, 0, 1.0, 1.0));
            AbsoluteLayout.SetLayoutFlags(_background, AbsoluteLayoutFlags.SizeProportional);

            _grid = new Grid()
            {
                BackgroundColor = Color.Transparent,
                Margin = Padding,
            };

            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20, GridUnitType.Absolute) });

            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            _grid.SizeChanged += GridSizeChanged;

            AbsoluteLayout.SetLayoutBounds(_grid, new Rectangle(0, 0, 1.0, 1.0));
            AbsoluteLayout.SetLayoutFlags(_grid, AbsoluteLayoutFlags.SizeProportional);

            _close = new Button
            {
                BackgroundColor = Color.Transparent,
                WidthRequest = 16,
                HeightRequest = 16,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                Command = new Command(() => CloseCalloutClicked(this, new EventArgs())),
            };

            _title = new Label
            {
                Text = string.Empty,
                LineBreakMode = LineBreakMode.NoWrap,
                FontFamily = "Arial",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.Transparent,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start,
            };

            _subtitle = new Label
            {
                Text = string.Empty,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.Start,
                FontFamily = "Arial",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.Transparent,
            };

            _grid.Children.Add(_close, 1, 0);
            _grid.Children.Add(_title, 0, 0);
            _grid.Children.Add(_subtitle, 0, 1);
            Grid.SetColumnSpan(_subtitle, 2);

            _content = new ContentView
            {
                IsVisible = false,
            };

            base.Content = new AbsoluteLayout
            {
                Children = {
                    _background,
                    _grid,
                    _content,
                }
            };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => HandleCalloutClicked(s, e);
            GestureRecognizers.Add(tapGestureRecognizer);

            SizeChanged += CalloutSizeChanged;

            UpdateGridSize();
            UpdatePath();
            UpdateMargin();
        }

        // TODO: Remove events when disposing

        /// <summary>
        /// Type of Callout
        /// </summary>
        public CalloutType Type
        {
            get { return (CalloutType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        /// <summary>
        /// Anchor position of Callout
        /// </summary>
        public Position Anchor
        {
            get { return (Position)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }

        /// <summary>
        /// Anchor position of Callout
        /// </summary>
        public ArrowAlignment ArrowAlignment
        {
            get { return (ArrowAlignment)GetValue(ArrowAlignmentProperty); }
            set { SetValue(ArrowAlignmentProperty, value); }
        }

        /// <summary>
        /// Width of opening of anchor of Callout
        /// </summary>
        public float ArrowWidth
        {
            get { return (float)GetValue(ArrowWidthProperty); }
            set { SetValue(ArrowWidthProperty, value); }
        }

        /// <summary>
        /// Height of anchor of Callout
        /// </summary>
        public float ArrowHeight
        {
            get { return (float)GetValue(ArrowHeightProperty); }
            set { SetValue(ArrowHeightProperty, value); }
        }

        /// <summary>
        /// Relative position of anchor of Callout on the side given by AnchorType
        /// </summary>
        public float ArrowPosition
        {
            get { return (float)GetValue(ArrowPositionProperty); }
            set { SetValue(ArrowPositionProperty, value); }
        }

        /// <summary>
        /// Color of stroke around Callout
        /// </summary>
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// BackgroundColor of Callout
        /// </summary>
        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        /// <summary>
        /// Stroke width of frame around Callout
        /// </summary>
        public float StrokeWidth
        {
            get { return (float)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        /// <summary>
        /// Radius of rounded corners of Callout
        /// </summary>
        public float RectRadius
        {
            get { return (float)GetValue(RectRadiusProperty); }
            set { SetValue(RectRadiusProperty, value); }
        }

        /// <summary>
        /// Padding around content of Callout
        /// </summary>
        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Is Callout visible on map
        /// </summary>
        public new bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            private set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// Is close button to hide Callout visible
        /// </summary>
        public bool IsCloseVisible
        {
            get { return (bool)GetValue(IsCloseVisibleProperty); }
            set { SetValue(IsCloseVisibleProperty, value); }
        }

        /// <summary>
        /// Closes Callout by clicking somewhere else on the MapView
        /// </summary>
        public bool IsClosableByClick
        {
            get { return (bool)GetValue(IsClosableByClickProperty); }
            set { SetValue(IsClosableByClickProperty, value); }
        }

        /// <summary>
        /// Content of Callout
        /// </summary>
        public new ContentView Content
        {
            get { return (ContentView)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Label for header
        /// </summary>
        public Label TitleLabel
        {
            get { return _title; }
        }

        /// <summary>
        /// Label for detail
        /// </summary>
        public Label SubtitleLabel
        {
            get { return _subtitle; }
        }

        /// <summary>
        /// Content of Callout header
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Content of Callout detail label
        /// </summary>
        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        internal void Show()
        {
            // Add all event handlers
            _mapControl.SizeChanged += MapControlSizeChanged;
            _mapControl.Viewport.ViewportChanged += ViewportChanged;

            UpdateScreenPosition();

            IsVisible = true;
        }

        internal void Hide()
        {
            IsVisible = false;

            // Remove all event handlers
            _mapControl.SizeChanged -= MapControlSizeChanged;
            _mapControl.Viewport.ViewportChanged -= ViewportChanged;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            Device.BeginInvokeOnMainThread(() => base.OnPropertyChanged(propertyName));

            if (propertyName.Equals(nameof(Title)))
            {
                _title.Text = Title;
            }

            if (propertyName.Equals(nameof(Subtitle)))
            {
                _subtitle.Text = Subtitle;
            }

            if (propertyName.Equals(nameof(Content)))
            {
                UpdateContent();
            }

            if (propertyName.Equals(nameof(IsCloseVisible)))
            {
                UpdateContent();
            }

            if (propertyName.Equals(nameof(Type)))
            {
                UpdateContent();
            }

            if (propertyName.Equals(nameof(ArrowAlignment))
                || propertyName.Equals(nameof(ArrowWidth))
                || propertyName.Equals(nameof(ArrowHeight))
                || propertyName.Equals(nameof(ArrowPosition))
                || propertyName.Equals(nameof(Padding)))
            {
                UpdateMargin();
                UpdatePath();
                _background?.InvalidateSurface();
            }

            if (propertyName.Equals(nameof(Padding)))
            {
                _grid.Margin = Padding;
                UpdateMargin();
                UpdateGridSize();
                _background?.InvalidateSurface();
            }

            if (propertyName.Equals(nameof(Color)) 
                || propertyName.Equals(nameof(BackgroundColor)))
            {
                _background?.InvalidateSurface();
            }

            if (propertyName.Equals(nameof(RectRadius)))
            {
                UpdatePath();
                _background?.InvalidateSurface();
            }
        }

        /// <summary>
        /// Callout is touched
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">SKTouchEventArgs</param>
        private void HandleCalloutClicked(object sender, EventArgs e)
        {
            CalloutClicked?.Invoke(this, e);
        }

        /// <summary>
        /// Create callout outline in background
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void HandlePaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            canvas.Scale(_mapControl.SkiaScale, _mapControl.SkiaScale);

            var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _shadowWidth)};
            var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = BackgroundColor.ToSKColor() };
            var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = Color.ToSKColor(), StrokeWidth = StrokeWidth };

            if (_path == null)
                UpdatePath();

            canvas.Clear(SKColors.Transparent);
            canvas.DrawPath(_path, shadow);
            canvas.DrawPath(_path, fill);
            canvas.DrawPath(_path, stroke);

            // Draw close button
            if (IsCloseVisible)
            {
                var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.DarkGray, StrokeWidth = 2 };
                var pos = _close.Bounds.Offset(_grid.Bounds.Left, _grid.Bounds.Top).Inflate(-4, -4);
                canvas.DrawLine((float)pos.Left, (float)pos.Top, (float)pos.Right, (float)pos.Bottom, paint);
                canvas.DrawLine((float)pos.Left, (float)pos.Bottom, (float)pos.Right, (float)pos.Top, paint);
            }
        }

        /// <summary>
        /// Something outside changed, so update screen position
        /// </summary>
        internal void UpdateScreenPosition()
        {
            // Don't update screen position, if there isn't a layer
            if (_mapControl.Map.Layers.Count == 0)
                return;

            var point = _mapControl?.Viewport?.WorldToScreen(Anchor.ToMapsui());

            if (point != null)
                AbsoluteLayout.SetLayoutBounds(this, new Rectangle(point.X - _offset.X, point.Y - _offset.Y, Width, Height));
        }

        /// <summary>
        /// Checks type of Callout and activates correct content
        /// </summary>
        private void UpdateContent()
        {
            _close.IsVisible = IsCloseVisible;

            switch (Type)
            {
                case CalloutType.Single:
                    _grid.IsVisible = true;
                    _subtitle.IsVisible = false;
                    _content.IsVisible = false;
                    Grid.SetColumnSpan(_title, IsCloseVisible ? 1 : 2);
                    Grid.SetRowSpan(_title, 2);
                    break;
                case CalloutType.Detail:
                    _grid.IsVisible = true;
                    _subtitle.IsVisible = true;
                    _content.IsVisible = false;
                    Grid.SetColumnSpan(_title, IsCloseVisible ? 1 : 2);
                    Grid.SetColumnSpan(_subtitle, 2);
                    Grid.SetRowSpan(_title, 1);
                    break;
                case CalloutType.Custom:
                    if (_content != Content)
                    {
                        if (Content == null)
                        {
                            // Add a dummy view
                            _content = new ContentView
                            {
                                Content = new BoxView
                                {
                                    WidthRequest = 150,
                                    HeightRequest = 50,
                                    Color = BackgroundColor,
                                }
                            };
                        }
                        else
                            _content.Content = Content;
                    }
                    _grid.IsVisible = false;
                    _content.IsVisible = true;
                    break;
            }

            UpdatePath();
        }

        /// <summary>
        /// Resize the Callout, that grid is complete visible
        /// </summary>
        private void UpdateGridSize()
        {
            SizeRequest size;

            if (Type == CalloutType.Custom)
                size = _content.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
            else
                size = _grid.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

            // Calc new size of info window to hold the complete content. 
            // Add some extra amount to be sure, that it is big enough.
            var width = size.Request.Width + Padding.Left + Padding.Right + ((ArrowAlignment == ArrowAlignment.Left || ArrowAlignment == ArrowAlignment.Right) ? ArrowHeight : 0) + _shadowWidth * 2 + 4;
            var height = size.Request.Height + Padding.Top + Padding.Bottom + ((ArrowAlignment == ArrowAlignment.Top || ArrowAlignment == ArrowAlignment.Bottom) ? ArrowHeight : 0) + _shadowWidth * 2 + 4;

            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(X, Y, width, height));

            // Now, when we have updated info windows size, then we should update path
            UpdatePath();
        }

        /// <summary>
        /// Update margins of grid
        /// </summary>
        private void UpdateMargin()
        {
            var margin = Padding;

            switch (ArrowAlignment)
            {
                case ArrowAlignment.Bottom:
                    margin.Bottom += ArrowHeight;
                    break;
                case ArrowAlignment.Top:
                    margin.Top += ArrowHeight;
                    break;
                case ArrowAlignment.Left:
                    margin.Left += ArrowHeight;
                    break;
                case ArrowAlignment.Right:
                    margin.Right += ArrowHeight;
                    break;
            }

            margin.Left += _shadowWidth;
            margin.Top += _shadowWidth;
            margin.Right += _shadowWidth;
            margin.Bottom += _shadowWidth;

            _grid.Margin = margin;
            _content.Margin = margin;

            UpdatePath();
        }

        /// <summary>
        /// Update path
        /// </summary>
        private void UpdatePath()
        {
            var width = Width - _shadowWidth * 2;
            var height = Height - _shadowWidth * 2;
            var halfWidth = width * ArrowPosition;
            var halfHeight = height * ArrowPosition;
            var bottom = (float)height - _shadowWidth;
            var left = _shadowWidth;
            var top = _shadowWidth;
            var right = (float)width - _shadowWidth;
            var start = new Point();
            var center = new Point();
            var end = new Point();

            // Check, if we are to near of the corners
            if (halfWidth - ArrowWidth * 0.5 < RectRadius)
                halfWidth = ArrowWidth * 0.5 + RectRadius;
            if (halfWidth + ArrowWidth * 0.5 > width - RectRadius)
                halfWidth = width - ArrowWidth * 0.5 - RectRadius;
            if (halfHeight - ArrowWidth * 0.5 < RectRadius)
                halfHeight = ArrowWidth * 0.5 + RectRadius;
            if (halfHeight + ArrowWidth * 0.5 > height - RectRadius)
                halfHeight = height - ArrowWidth * 0.5 - RectRadius;
            
            switch (ArrowAlignment)
            {
                case ArrowAlignment.Bottom:
                    start = new Point(halfWidth + ArrowWidth * 0.5, bottom - ArrowHeight);
                    center = new Point(halfWidth, bottom);
                    end = new Point(halfWidth - ArrowWidth * 0.5, bottom - ArrowHeight);
                    bottom -= ArrowHeight;
                    break;
                case ArrowAlignment.Top:
                    start = new Point(halfWidth - ArrowWidth * 0.5, top + ArrowHeight);
                    center = new Point(halfWidth, top);
                    end = new Point(halfWidth + ArrowWidth * 0.5, top + ArrowHeight);
                    top += ArrowHeight;
                    break;
                case ArrowAlignment.Left:
                    start = new Point(left + ArrowHeight, halfHeight + ArrowWidth * 0.5);
                    center = new Point(left, halfHeight);
                    end = new Point(left + ArrowHeight, halfHeight - ArrowWidth * 0.5);
                    left += ArrowHeight;
                    break;
                case ArrowAlignment.Right:
                    start = new Point(right - ArrowHeight, halfHeight - ArrowWidth * 0.5);
                    center = new Point(right, halfHeight);
                    end = new Point(right - ArrowHeight, halfHeight + ArrowWidth * 0.5);
                    right -= ArrowHeight;
                    break;
            }

            // Create path
            _path = new SKPath();

            // Move to start point at left/top
            _path.MoveTo(left + RectRadius, top);

            // Top horizontal line
            if (ArrowAlignment == ArrowAlignment.Top)
                DrawArrow(start, center, end);

            // Top right arc
            _path.ArcTo(new SKRect(right - RectRadius, top, right, top + RectRadius), 270, 90, false);

            // Right vertical line
            if (ArrowAlignment == ArrowAlignment.Right)
                DrawArrow(start, center, end);

            // Bottom right arc
            _path.ArcTo(new SKRect(right - RectRadius, bottom - RectRadius, right, bottom), 0, 90, false);

            // Bottom horizontal line
            if (ArrowAlignment == ArrowAlignment.Bottom)
                DrawArrow(start, center, end);

            // Bottom left arc
            _path.ArcTo(new SKRect(left, bottom - RectRadius, left + RectRadius, bottom), 90, 90, false);

            // Left vertical line
            if (ArrowAlignment == ArrowAlignment.Left)
                DrawArrow(start, center, end);

            // Top left arc
            _path.ArcTo(new SKRect(left, top, left + RectRadius, top + RectRadius), 180, 90, false);

            _path.Close();

            // Set center as new anchor point
            _offset = center;

            // We changed so much, so update screen position
            UpdateScreenPosition();
        }

        /// <summary>
        /// Draw arrow to path
        /// </summary>
        /// <param name="start">Start of arrow at bubble</param>
        /// <param name="center">Center of arrow</param>
        /// <param name="end">End of arrow at bubble</param>
        private void DrawArrow(Point start, Point center, Point end)
        {
            _path.LineTo(start.ToSKPoint());
            _path.LineTo(center.ToSKPoint());
            _path.LineTo(end.ToSKPoint());
        }

        /// <summary>
        /// Size changed, so recalc all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalloutSizeChanged(object sender, EventArgs e)
        {
            UpdateMargin();
            UpdatePath();
        }

        /// <summary>
        /// Size of grid changed, so update size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSizeChanged(object sender, EventArgs e)
        {
            UpdateGridSize();
        }

        private void ViewportChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Viewport.Center)))
                UpdateScreenPosition();
        }

        private void MapControlSizeChanged(object sender, EventArgs e)
        {
            UpdateScreenPosition();
        }

        /// <summary>
        /// Called, when Callout close button is pressed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void CloseCalloutClicked(object sender, EventArgs e)
        {
            CalloutClosed?.Invoke(this, new EventArgs());
        }
    }
}
