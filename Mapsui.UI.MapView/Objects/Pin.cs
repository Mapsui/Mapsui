using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Objects;
using Mapsui.Utilities;
using SkiaSharp;
#if __MAUI__
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui;

using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

using Color = Xamarin.Forms.Color;
using KnownColor = Xamarin.Forms.Color;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

public class Pin : BindableObject, IFeatureProvider, IDisposable
{
    // Cache for used bitmaps
    private static readonly Dictionary<string, int> _bitmapIds = new Dictionary<string, int>();

    private string _bitmapIdKey = string.Empty; // Key for active _bitmapIds entry
    private int _bitmapId = -1;
    private byte[]? _bitmapData;
    private MapView? _mapView;

    public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(PinType), typeof(Pin), default(PinType));
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(Position), typeof(Pin), default(Position));
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));
    public static readonly BindableProperty AddressProperty = BindableProperty.Create(nameof(Address), typeof(string), typeof(Pin), default(string));
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(byte[]), typeof(Pin), default(byte[]));
    public static readonly BindableProperty SvgProperty = BindableProperty.Create(nameof(Svg), typeof(string), typeof(Pin), default(string));
    public static readonly BindableProperty ScaleProperty = BindableProperty.Create(nameof(Scale), typeof(float), typeof(Pin), 1.0f);
    public static readonly BindableProperty RotationProperty = BindableProperty.Create(nameof(Rotation), typeof(float), typeof(Pin), 0f);
    public static readonly BindableProperty RotateWithMapProperty = BindableProperty.Create(nameof(RotateWithMap), typeof(bool), typeof(Pin), false);
    public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(Pin), true);
    public static readonly BindableProperty MinVisibleProperty = BindableProperty.Create(nameof(MinVisible), typeof(double), typeof(Pin), 0.0);
    public static readonly BindableProperty MaxVisibleProperty = BindableProperty.Create(nameof(MaxVisible), typeof(double), typeof(Pin), double.MaxValue);
    public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(double), typeof(Pin), -1.0, BindingMode.OneWayToSource);
    public static readonly BindableProperty HeightProperty = BindableProperty.Create(nameof(Height), typeof(double), typeof(Pin), -1.0);
    public static readonly BindableProperty AnchorProperty = BindableProperty.Create(nameof(Anchor), typeof(Point), typeof(Pin), new Point(0, 28));
    public static readonly BindableProperty TransparencyProperty = BindableProperty.Create(nameof(Transparency), typeof(float), typeof(Pin), 0f);
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(Pin), KnownColor.Red);

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Pin"/> class
    /// </summary>
    /// <param name="mapView">MapView to which this pin belongs</param>
    public Pin(MapView mapView)
    {
        _mapView = mapView;

        CreateFeature();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Pin"/> class
    /// </summary>
    public Pin()
    {
    }

    /// <summary>
    /// Internal MapView for refreshing of screen
    /// </summary>
    internal MapView? MapView
    {
        get => _mapView;
        set
        {
            if (_mapView != value)
            {
                if (_callout != null)
                {
                    _mapView?.RemoveCallout(_callout);
                }

                Feature?.Dispose();
                Feature = null;
                _mapView = value;

                CreateFeature();
            }
        }
    }

    /// <summary>
    /// Type of pin. There are some predefined pins.
    /// </summary>
    public PinType Type
    {
        get => (PinType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// Position of pin, place where anchor is
    /// </summary>
    public Position Position
    {
        get => (Position)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>
    /// Scaling of pin
    /// </summary>
    public float Scale
    {
        get => (float)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    /// <summary>
    /// Color of pin
    /// </summary>
    public Color Color
    {
        get { return (Color)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }

    /// <summary>
    /// Label of pin
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Adress (like street) of pin
    /// </summary>
    public string Address
    {
        get => (string)GetValue(AddressProperty);
        set => SetValue(AddressProperty, value);
    }

    /// <summary>
    /// Byte[] holding the bitmap informations
    /// </summary>
    public byte[] Icon
    {
        get => (byte[])GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// String holding the Svg image informations
    /// </summary>
    public string Svg
    {
        get => (string)GetValue(SvgProperty);
        set => SetValue(SvgProperty, value);
    }

    /// <summary>
    /// Rotation in degrees around the anchor point
    /// </summary>
    public float Rotation
    {
        get => (float)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    /// <summary>
    /// When true a symbol will rotate along with the rotation of the map.
    /// The default is false.
    /// </summary>
    public bool RotateWithMap
    {
        get => (bool)GetValue(RotateWithMapProperty);
        set => SetValue(RotateWithMapProperty, value);
    }

    /// <summary>
    /// Determins, if the pin is drawn on map
    /// </summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// MinVisible for pin in resolution of Mapsui (smaller values are higher zoom levels)
    /// </summary>
    public double MinVisible
    {
        get => (double)GetValue(MinVisibleProperty);
        set => SetValue(MinVisibleProperty, value);
    }

    /// <summary>
    /// MaxVisible for pin in resolution of Mapsui (smaller values are higher zoom levels)
    /// </summary>
    public double MaxVisible
    {
        get => (double)GetValue(MaxVisibleProperty);
        set => SetValue(MaxVisibleProperty, value);
    }

    /// <summary>
    /// Width of the bitmap after scaling, if there is one, if not, than -1
    /// </summary>
    public double Width
    {
        get => (double)GetValue(WidthProperty);
        private set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Height of the bitmap after scaling, if there is one, if not, than -1
    /// </summary>
    public double Height
    {
        get => (double)GetValue(HeightProperty);
        private set => SetValue(HeightProperty, value);
    }

    /// <summary>
    /// Anchor of bitmap in pixel
    /// </summary>
    public Point Anchor
    {
        get => (Point)GetValue(AnchorProperty);
        set => SetValue(AnchorProperty, value);
    }

    /// <summary>
    /// Transparency of pin
    /// </summary>
    public float Transparency
    {
        get => (float)GetValue(TransparencyProperty);
        set => SetValue(TransparencyProperty, value);
    }

    /// <summary>
    /// Tag holding free data
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Mapsui feature for this pin
    /// </summary>
    /// <value>Mapsui feature</value>
    public GeometryFeature? Feature { get; private set; }

    private Callout? _callout;

    /// <summary>
    /// Gets the callout
    /// </summary>
    /// <value>Callout for this pin</value>
    public Callout Callout
    {
        get
        {
            // Show a new Callout
            if (_callout == null)
            {
                // Create a default callout
                _callout = new Callout(this);
                if (string.IsNullOrWhiteSpace(Address))
                {
                    _callout.Type = CalloutType.Single;
                    _callout.Title = Label;
                }
                else
                {
                    _callout.Type = CalloutType.Detail;
                    _callout.Title = Label;
                    _callout.Subtitle = Address;
                }
            }

            return _callout;
        }
        internal set
        {
            if (value != null && _callout != value)
                _callout = value;
        }
    }

    /// <summary>
    /// Show corresponding callout
    /// </summary>
    public void ShowCallout()
    {
        if (_callout != null)
        {
            _callout.Update();
            _mapView?.AddCallout(_callout);
        }
    }

    /// <summary>
    /// Hide corresponding callout
    /// </summary>
    public void HideCallout()
    {
        _mapView?.RemoveCallout(_callout);
    }

    /// <summary>
    /// Check visibility for corresponding callout
    /// </summary>
    /// <returns>True, if callout is visible on map</returns>
    public bool IsCalloutVisible()
    {
        return _mapView != null && _callout != null && _mapView.IsCalloutVisible(_callout);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Mapsui.UI.Forms.Pin"/>.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:Mapsui.UI.Forms.Pin"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
    /// <see cref="T:Mapsui.UI.Forms.Pin"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((Pin)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Label?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ Position.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)Type;
            hashCode = (hashCode * 397) ^ (Address?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

    public static bool operator ==(Pin? left, Pin? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Pin? left, Pin? right)
    {
        return !Equals(left, right);
    }

    private bool Equals(Pin? other)
    {
        if (other == null)
            return false;

        return string.Equals(Label, other.Label) && Equals(Position, other.Position) && Type == other.Type && string.Equals(Address, other.Address);
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(Position):
                if (Feature != null)
                {
                    Feature.Geometry = Position.ToPoint();
                    if (_callout != null)
                        _callout.Feature.Geometry = Feature.Geometry;
                }
                break;
            case nameof(Label):
                if (Feature != null)
                    Feature["Label"] = Label;
                Callout.Title = Label;
                break;
            case nameof(Address):
                Callout.Subtitle = Address;
                break;
            case nameof(Transparency):
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).Opacity = 1 - Transparency;
                break;
            case nameof(Anchor):
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).SymbolOffset = new Offset(Anchor.X, Anchor.Y);
                break;
            case nameof(Rotation):
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).SymbolRotation = Rotation;
                break;
            case nameof(RotateWithMap):
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).RotateWithMap = RotateWithMap;
                break;
            case nameof(IsVisible):
                if (!IsVisible)
                    HideCallout();
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).Enabled = IsVisible;
                break;
            case nameof(MinVisible):
                // TODO: Update callout MinVisble too
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).MinVisible = MinVisible;
                break;
            case nameof(MaxVisible):
                // TODO: Update callout MaxVisble too
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).MaxVisible = MaxVisible;
                break;
            case nameof(Scale):
                if (Feature != null)
                    ((SymbolStyle)Feature.Styles.First()).SymbolScale = Scale;
                break;
            case nameof(Type):
            case nameof(Color):
                CreateFeature();
                break;
            case nameof(Icon):
                if (Type == PinType.Icon)
                    CreateFeature();
                break;
            case nameof(Svg):
                if (Type == PinType.Svg)
                    CreateFeature();
                break;
        }
    }

    private readonly object _sync = new object();

    private void CreateFeature()
    {
        lock (_sync)
        {
            if (Feature == null)
            {
                // Create a new one
                Feature = new GeometryFeature
                {
                    Geometry = Position.ToPoint(),
                    ["Label"] = Label,
                };
                if (_callout != null)
                    _callout.Feature.Geometry = Position.ToPoint();
            }
            // Check for bitmapId
            if (_bitmapId != -1)
            {
                // There is already a registered bitmap, so delete it
                _bitmapId = -1;
                _bitmapIdKey = string.Empty;
            }

            switch (Type)
            {
                case PinType.Svg:
                    // Load the SVG document
                    if (string.IsNullOrEmpty(Svg))
                        return;
                    // Check, if it is already in cache
                    if (_bitmapIds.ContainsKey(Svg))
                    {
                        _bitmapId = _bitmapIds[Svg];
                        _bitmapIdKey = Svg;
                        break;
                    }
                    // Save this SVG for later use
                    _bitmapId = BitmapRegistry.Instance.Register(Svg);
                    _bitmapIdKey = Svg;
                    _bitmapIds.Add(Svg, _bitmapId);
                    break;
                case PinType.Pin:
                    var colorInHex = Color.ToHex();
                    // Check, if it is already in cache
                    if (_bitmapIds.ContainsKey(colorInHex))
                    {
                        _bitmapId = _bitmapIds[colorInHex];
                        _bitmapIdKey = colorInHex;
                        break;
                    }

                    // Load the SVG document
                    Svg.Skia.SKSvg svg;
                    using (var stream = Utilities.EmbeddedResourceLoader.Load("Images.Pin.svg", typeof(Pin)))
                    {
                        if (stream == null)
                            return;

                        // Create a new SVG object
                        svg = stream.LoadSvg();
                        if (svg.Picture == null)
                            return;
                    }

                    Width = svg.Picture.CullRect.Width * Scale;
                    Height = svg.Picture.CullRect.Height * Scale;
                    // Create bitmap to hold canvas
                    var info = new SKImageInfo((int)svg.Picture.CullRect.Width, (int)svg.Picture.CullRect.Height) { AlphaType = SKAlphaType.Premul };
                    using (var bitmap = new SKBitmap(info))
                    {
                        using var canvas = new SKCanvas(bitmap);
                        // Now draw Svg image to bitmap
                        using (var paint = new SKPaint() { IsAntialias = true })
                        {
                            // Replace color while drawing
                            paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn); // use the source color
                            canvas.Clear();
                            canvas.DrawPicture(svg.Picture, paint);
                        }
                        // Now convert canvas to bitmap
                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            _bitmapData = data.ToArray();
                        }
                        _bitmapId = BitmapRegistry.Instance.Register(_bitmapData);
                        _bitmapIdKey = colorInHex;
                        _bitmapIds.Add(colorInHex, _bitmapId);
                    }

                    break;
                case PinType.Icon:
                    if (Icon != null)
                    {
                        using (var image = SKBitmap.Decode(Icon))
                        {
                            Width = image.Width * Scale;
                            Height = image.Height * Scale;
                            _bitmapId = BitmapRegistry.Instance.Register(Icon);
                        }
                    }
                    break;
            }

            // If we have a bitmapId (and we should have one), than draw bitmap, otherwise nothing
            if (_bitmapId != -1)
            {
                // We only want to have one style
                Feature.Styles.Clear();
                Feature.Styles.Add(new SymbolStyle
                {
                    BitmapId = _bitmapId,
                    SymbolScale = Scale,
                    SymbolRotation = Rotation,
                    RotateWithMap = RotateWithMap,
                    SymbolOffset = new Offset(Anchor.X, Anchor.Y),
                    Opacity = 1 - Transparency,
                    Enabled = IsVisible,
                });
            }
        }
    }

    public virtual void Dispose()
    {
        _callout?.Dispose();
        Feature?.Dispose();
    }
}
