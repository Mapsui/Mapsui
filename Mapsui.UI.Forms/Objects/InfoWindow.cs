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
    /// Type of InfoWindow
    /// </summary>
    public enum InfoWindowType
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
    public enum ArrowLocationType
    {
        Bottom,
        Left,
        Top,
        Right,
    }

    public class InfoWindow : ContentView
    {
        private MapControl _mapControl;
        private SKCanvasView _background;
        private Grid _grid;
        private Label _header;
        private Label _detail;
        private ContentView _content;
        private Image _close;
        private SKPath _path;
        private Point _offset;

        public event EventHandler<EventArgs> InfoWindowClosed;

        public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(InfoWindowType), typeof(MapView), default(InfoWindowType));
        public static readonly BindableProperty AnchorProperty = BindableProperty.Create(nameof(Anchor), typeof(Position), typeof(MapView), default(Position));
        public static readonly BindableProperty ArrowLocationProperty = BindableProperty.Create(nameof(ArrowLocation), typeof(ArrowLocationType), typeof(MapView), default(ArrowLocationType), defaultBindingMode: BindingMode.TwoWay);
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
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MapView), default(string));
        public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(MapView), default(string));

        public InfoWindow(MapControl mapControl)
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

            _background.PaintSurface += PaintSurface;

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

            // TODO: Remove
            var assembly = typeof(InfoWindow).GetTypeInfo().Assembly;
            foreach (var s in assembly.GetManifestResourceNames())
                System.Diagnostics.Debug.WriteLine(s);

            _close = new Image
            {
                Source = ImageSource.FromResource("Mapsui.UI.Images.Close.png", typeof(InfoWindow).GetTypeInfo().Assembly),
                BackgroundColor = Color.Transparent,
                Margin = new Thickness(4),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
            };

            // Use image as button
            _close.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() => CloseInfoWindowClicked(this, new EventArgs()))
            });

            _header = new Label
            {
                Text = "Test", //string.Empty,
                LineBreakMode = LineBreakMode.NoWrap,
                FontFamily = "Arial",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.Transparent,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Start,
            };

            _detail = new Label
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
            _grid.Children.Add(_header, 0, 0);
            _grid.Children.Add(_detail, 0, 1);
            Grid.SetColumnSpan(_detail, 2);

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

            SizeChanged += InfoWindowSizeChanged;

            UpdateGridSize();
            UpdatePath();
            UpdateMargin();
        }

        // TODO: Remove events when disposing

        /// <summary>
        /// Type of InfoWindow
        /// </summary>
        public InfoWindowType Type
        {
            get { return (InfoWindowType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        /// <summary>
        /// Anchor position of InfoWindow
        /// </summary>
        public Position Anchor
        {
            get { return (Position)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }

        /// <summary>
        /// Anchor position of InfoWindow
        /// </summary>
        public ArrowLocationType ArrowLocation
        {
            get { return (ArrowLocationType)GetValue(ArrowLocationProperty); }
            set { SetValue(ArrowLocationProperty, value); }
        }

        /// <summary>
        /// Width of opening of anchor of InfoWindow
        /// </summary>
        public float ArrowWidth
        {
            get { return (float)GetValue(ArrowWidthProperty); }
            set { SetValue(ArrowWidthProperty, value); }
        }

        /// <summary>
        /// Height of anchor of InfoWindow
        /// </summary>
        public float ArrowHeight
        {
            get { return (float)GetValue(ArrowHeightProperty); }
            set { SetValue(ArrowHeightProperty, value); }
        }

        /// <summary>
        /// Relative position of anchor of InfoWindow on the side given by AnchorType
        /// </summary>
        public float ArrowPosition
        {
            get { return (float)GetValue(ArrowPositionProperty); }
            set { SetValue(ArrowPositionProperty, value); }
        }

        /// <summary>
        /// Color of stroke around InfoWindow
        /// </summary>
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// BackgroundColor of InfoWindow
        /// </summary>
        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        /// <summary>
        /// Stroke width of frame around InfoWindow
        /// </summary>
        public float StrokeWidth
        {
            get { return (float)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        /// <summary>
        /// Radius of rounded corners of InfoWindow
        /// </summary>
        public float RectRadius
        {
            get { return (float)GetValue(RectRadiusProperty); }
            set { SetValue(RectRadiusProperty, value); }
        }

        /// <summary>
        /// Radius of rounded corners of InfoWindow
        /// </summary>
        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Is InfoWindow visible on map
        /// </summary>
        public new bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            private set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// Is close button to hide InfoWindow visible
        /// </summary>
        public bool IsCloseVisible
        {
            get { return (bool)GetValue(IsCloseVisibleProperty); }
            set { SetValue(IsCloseVisibleProperty, value); }
        }

        /// <summary>
        /// Closes InfoWindow by clicking some else on the MapView
        /// </summary>
        public bool IsClosableByClick
        {
            get { return (bool)GetValue(IsClosableByClickProperty); }
            set { SetValue(IsClosableByClickProperty, value); }
        }

        /// <summary>
        /// Content of InfoWindow
        /// </summary>
        public new ContentView Content
        {
            get { return (ContentView)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Label for header
        /// </summary>
        public Label TextLabel
        {
            get { return _header; }
        }

        /// <summary>
        /// Label for detail
        /// </summary>
        public Label DetailLabel
        {
            get { return _detail; }
        }

        /// <summary>
        /// Content of InfoWindow header
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Content of InfoWindow detail label
        /// </summary>
        public string Detail
        {
            get { return (string)GetValue(DetailProperty); }
            set { SetValue(DetailProperty, value); }
        }

        internal void Show()
        {
            // Add all event handlers
            _mapControl.SizeChanged += MapControlSizeChanged;
            _mapControl.Map.Viewport.ViewportChanged += ViewportChanged;

            UpdateScreenPosition();

            IsVisible = true;
        }

        internal void Hide()
        {
            IsVisible = false;

            // Remove all event handlers
            _mapControl.SizeChanged -= MapControlSizeChanged;
            _mapControl.Map.Viewport.ViewportChanged -= ViewportChanged;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            Device.BeginInvokeOnMainThread(() => base.OnPropertyChanged(propertyName));

            if (propertyName.Equals(nameof(Text)))
            {
                _header.Text = Text;
            }

            if (propertyName.Equals(nameof(Detail)))
            {
                _detail.Text = Detail;
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

            if (propertyName.Equals(nameof(ArrowLocation))
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
        /// Paint bubble in background
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            canvas.Scale(_mapControl.SkiaScale, _mapControl.SkiaScale);

            var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = BackgroundColor.ToSKColor() };
            var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = Color.ToSKColor(), StrokeWidth = StrokeWidth };

            if (_path == null)
                UpdatePath();

            canvas.Clear(SKColors.Transparent);
            canvas.DrawPath(_path, fill);
            canvas.DrawPath(_path, stroke);
        }

        /// <summary>
        /// Something outside changed, so update screen position
        /// </summary>
        private void UpdateScreenPosition()
        {
            // Don't update screen position, if there isn't a layer
            if (_mapControl.Map.Layers.Count == 0)
                return;

            var point = _mapControl?.Map?.Viewport?.WorldToScreen(Anchor.ToMapsui());

            if (point != null)
                AbsoluteLayout.SetLayoutBounds(this, new Rectangle(point.X - _offset.X, point.Y - _offset.Y, Width, Height));
        }

        /// <summary>
        /// Checks type of InfoWindow and activates correct content
        /// </summary>
        private void UpdateContent()
        {
            _close.IsVisible = IsCloseVisible;

            switch (Type)
            {
                case InfoWindowType.Single:
                    _grid.IsVisible = true;
                    _detail.IsVisible = false;
                    _content.IsVisible = false;
                    Grid.SetColumnSpan(_header, IsCloseVisible ? 1 : 2);
                    Grid.SetRowSpan(_header, 2);
                    break;
                case InfoWindowType.Detail:
                    _grid.IsVisible = true;
                    _detail.IsVisible = true;
                    _content.IsVisible = false;
                    Grid.SetColumnSpan(_header, IsCloseVisible ? 1 : 2);
                    Grid.SetColumnSpan(_detail, 2);
                    Grid.SetRowSpan(_header, 1);
                    break;
                case InfoWindowType.Custom:
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
        /// Resize the InfoWindow, that grid is complete visible
        /// </summary>
        private void UpdateGridSize()
        {
            SizeRequest size;

            if (Type == InfoWindowType.Custom)
                size = _content.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
            else
                size = _grid.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

            // Calc new size of info window to hold the complete content. Add some extra amount to be sure, that it is big enough.
            var width = size.Request.Width + Padding.Left + Padding.Right + ((ArrowLocation == ArrowLocationType.Left || ArrowLocation == ArrowLocationType.Right) ? ArrowHeight : 0) + 4;
            var height = size.Request.Height + Padding.Top + Padding.Bottom + ((ArrowLocation == ArrowLocationType.Top || ArrowLocation == ArrowLocationType.Bottom) ? ArrowHeight : 0) + 4;

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

            switch (ArrowLocation)
            {
                case ArrowLocationType.Bottom:
                    margin.Bottom += ArrowHeight;
                    break;
                case ArrowLocationType.Top:
                    margin.Top += ArrowHeight;
                    break;
                case ArrowLocationType.Left:
                    margin.Left += ArrowHeight;
                    break;
                case ArrowLocationType.Right:
                    margin.Right += ArrowHeight;
                    break;
            }

            _grid.Margin = margin;
            _content.Margin = margin;

            UpdatePath();
        }

        /// <summary>
        /// Update path
        /// </summary>
        private void UpdatePath()
        {
            var halfWidth = Width * ArrowPosition;
            var halfHeight = Height * ArrowPosition;
            var bottom = (float)Height;
            var left = 0.0f;
            var top = 0.0f;
            var right = (float)Width;
            var start = new Point();
            var center = new Point();
            var end = new Point();

            // Check, if we are to near of the corners
            if (halfWidth - ArrowWidth * 0.5 < RectRadius)
                halfWidth = ArrowWidth * 0.5 + RectRadius;
            if (halfWidth + ArrowWidth * 0.5 > Width - RectRadius)
                halfWidth = Width - ArrowWidth * 0.5 - RectRadius;
            if (halfHeight - ArrowWidth * 0.5 < RectRadius)
                halfHeight = ArrowWidth * 0.5 + RectRadius;
            if (halfHeight + ArrowWidth * 0.5 > Height - RectRadius)
                halfHeight = Height - ArrowWidth * 0.5 - RectRadius;
            
            switch (ArrowLocation)
            {
                case ArrowLocationType.Bottom:
                    start = new Point(halfWidth - ArrowWidth * 0.5, Height - ArrowHeight);
                    center = new Point(halfWidth, Height);
                    end = new Point(halfWidth + ArrowWidth * 0.5, Height - ArrowHeight);
                    bottom -= ArrowHeight;
                    break;
                case ArrowLocationType.Top:
                    start = new Point(halfWidth + ArrowWidth * 0.5, ArrowHeight);
                    center = new Point(halfWidth, 0);
                    end = new Point(halfWidth - ArrowWidth * 0.5, ArrowHeight);
                    top += ArrowHeight;
                    break;
                case ArrowLocationType.Left:
                    start = new Point(ArrowHeight, halfHeight - ArrowWidth * 0.5);
                    center = new Point(0, halfHeight);
                    end = new Point(ArrowHeight, halfHeight + ArrowWidth * 0.5);
                    left += ArrowHeight;
                    break;
                case ArrowLocationType.Right:
                    start = new Point(Width - ArrowHeight, halfHeight + ArrowWidth * 0.5);
                    center = new Point(Width, halfHeight);
                    end = new Point(Width - ArrowHeight, halfHeight - ArrowWidth * 0.5);
                    right -= ArrowHeight;
                    break;
            }

            // Create path for rectangle
            var rect = new SKPath();
            rect.AddRoundedRect(new SKRect(left, top, right, bottom), RectRadius, RectRadius, SKPathDirection.CounterClockwise);
            rect.Close();

            // Create path
            _path = new SKPath();

            _path.MoveTo(start.ToSKPoint());
            _path.LineTo(center.ToSKPoint());
            _path.LineTo(end.ToSKPoint());

            // Combine both path
            _path.Op(rect, SKPathOp.Union, _path);
            _path.Close();

            // Set center as new anchor point
            _offset = center;

            // We changed so much, so update screen position
            UpdateScreenPosition();
        }

        /// <summary>
        /// Size changed, so recalc all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoWindowSizeChanged(object sender, EventArgs e)
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
        /// Called, when InfoWindow close button is pressed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void CloseInfoWindowClicked(object sender, EventArgs e)
        {
            InfoWindowClosed?.Invoke(this, new EventArgs());
        }
    }
}
