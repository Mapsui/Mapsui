﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.UI.Objects;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Svg.Skia;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public class Pin : BindableObject, IFeatureProvider
    {
        // Cache for used bitmaps
        private static Dictionary<string, int> _bitmapIds = new Dictionary<string, int>();

        private string _bitmapIdKey = string.Empty; // Key for active _bitmapIds entry
        private int _bitmapId = -1;
        private MapView _mapView;

        public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(PinType), typeof(Pin), default(PinType));
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Xamarin.Forms.Color), typeof(Pin), SKColors.Red.ToFormsColor());
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
        /// <param name="mapView">MapView to which this pin belongs</param>
        public Pin()
        {
        }

        /// <summary>
        /// Internal MapView for refreshing of screen
        /// </summary>
        internal MapView MapView
        { 
            get 
            { 
                return _mapView; 
            } 
            set
            {
                if (_mapView != value)
                {
                    if (_callout != null)
                    {
                        _mapView?.RemoveCallout(_callout);
                    }
                    
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
            get { return (PinType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        /// <summary>
        /// Position of pin, place where anchor is
        /// </summary>
        public Position Position
        {
            get { return (Position)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Scaling of pin
        /// </summary>
        public float Scale
        {
            get { return (float)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        /// <summary>
        /// Color of pin
        /// </summary>
        public Xamarin.Forms.Color Color
        {
            get { return (Xamarin.Forms.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Label of pin
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Adress (like street) of pin
        /// </summary>
        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        /// <summary>
        /// Byte[] holding the bitmap informations
        /// </summary>
        public byte[] Icon
        {
            get { return (byte[])GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// String holding the Svg image informations
        /// </summary>
        public string Svg
        {
            get { return (string)GetValue(SvgProperty); }
            set { SetValue(SvgProperty, value); }
        }

        /// <summary>
        /// Rotation in degrees around the anchor point
        /// </summary>
        public float Rotation
        {
            get { return (float)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }

        /// <summary>
        /// When true a symbol will rotate along with the rotation of the map.
        /// The default is false.
        /// </summary>
        public bool RotateWithMap
        {
            get { return (bool)GetValue(RotateWithMapProperty); }
            set { SetValue(RotateWithMapProperty, value); }
        }

        /// <summary>
        /// Determins, if the pin is drawn on map
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// MinVisible for pin in resolution of Mapsui (smaller values are higher zoom levels)
        /// </summary>
        public double MinVisible
        {
            get { return (double)GetValue(MinVisibleProperty); }
            set { SetValue(MinVisibleProperty, value); }
        }

        /// <summary>
        /// MaxVisible for pin in resolution of Mapsui (smaller values are higher zoom levels)
        /// </summary>
        public double MaxVisible
        {
            get { return (double)GetValue(MaxVisibleProperty); }
            set { SetValue(MaxVisibleProperty, value); }
        }

        /// <summary>
        /// Width of the bitmap after scaling, if there is one, if not, than -1
        /// </summary>
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            private set { SetValue(WidthProperty, value); }
        }

        /// <summary>
        /// Height of the bitmap after scaling, if there is one, if not, than -1
        /// </summary>
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            private set { SetValue(HeightProperty, value); }
        }

        /// <summary>
        /// Anchor of bitmap in pixel
        /// </summary>
        public Point Anchor
        {
            get { return (Point)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }

        /// <summary>
        /// Transparency of pin
        /// </summary>
        public float Transparency
        {
            get { return (float)GetValue(TransparencyProperty); }
            set { SetValue(TransparencyProperty, value); }
        }

        /// <summary>
        /// Tag holding free data
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Mapsui feature for this pin
        /// </summary>
        /// <value>Mapsui feature</value>
        public Feature Feature { get; private set; }

        private Callout _callout;

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
            _callout.Update();
            _mapView.AddCallout(_callout);
        }

        /// <summary>
        /// Hide corresponding callout
        /// </summary>
        public void HideCallout()
        {
            _mapView.RemoveCallout(_callout);
        }

        /// <summary>
        /// Check visibility for corresponding callout
        /// </summary>
        /// <returns>True, if callout is visible on map</returns>
        public bool IsCalloutVisible()
        {
            return _mapView != null ? _mapView.IsCalloutVisible(_callout) : false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Mapsui.UI.Forms.Pin"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:Mapsui.UI.Forms.Pin"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Mapsui.UI.Forms.Pin"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
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
                int hashCode = Label?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ (Address?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(Pin left, Pin right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Pin left, Pin right)
        {
            return !Equals(left, right);
        }

        bool Equals(Pin other)
        {
            return string.Equals(Label, other.Label) && Equals(Position, other.Position) && Type == other.Type && string.Equals(Address, other.Address);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Position):
                    if (Feature != null)
                    {
                        Feature.Geometry = Position.ToMapsui();
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
                    ((SymbolStyle)Feature.Styles.First()).Opacity = 1 - Transparency;
                    break;
                case nameof(Anchor):
                    ((SymbolStyle)Feature.Styles.First()).SymbolOffset = new Offset(Anchor.X, Anchor.Y);
                    break;
                case nameof(Rotation):
                    ((SymbolStyle)Feature.Styles.First()).SymbolRotation = Rotation;
                    break;
                case nameof(RotateWithMap):
                    ((SymbolStyle)Feature.Styles.First()).RotateWithMap = RotateWithMap;
                    break;
                case nameof(IsVisible):
                    if (!IsVisible)
                        HideCallout();
                    ((SymbolStyle)Feature.Styles.First()).Enabled = IsVisible;
                    break;
                case nameof(MinVisible):
                    // TODO: Update callout MinVisble too
                    ((SymbolStyle)Feature.Styles.First()).MinVisible = MinVisible;
                    break;
                case nameof(MaxVisible):
                    // TODO: Update callout MaxVisble too
                    ((SymbolStyle)Feature.Styles.First()).MaxVisible = MaxVisible;
                    break;
                case nameof(Scale):
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
            SKSvg svg;

            lock (_sync)
            {
                if (Feature == null)
                {
                    // Create a new one
                    Feature = new Feature
                    {
                        Geometry = Position.ToMapsui(),
                        ["Label"] = Label,
                    };
                    if (_callout != null)
                        _callout.Feature.Geometry = Position.ToMapsui();
                }
                // Check for bitmapId
                if (_bitmapId != -1)
                {
                    // There is already a registered bitmap, so delete it
                    BitmapRegistry.Instance.Unregister(_bitmapId);
                    if (!string.IsNullOrEmpty(_bitmapIdKey))
                        _bitmapIds.Remove(_bitmapIdKey);
                    // We don't have any bitmap up to now
                    _bitmapId = -1;
                    _bitmapIdKey = string.Empty;
                }

                switch (Type)
                {
                    case PinType.Svg:
                        // Load the SVG document
                        if (string.IsNullOrEmpty(Svg))
                            return;
                        if (_bitmapIds.ContainsKey(Svg))
                        {
                            _bitmapId = _bitmapIds[Svg];
                            _bitmapIdKey = Svg;
                        }
                        else
                        {
                            if (Svg.ToLower().Contains("<svg"))
                            {
                                // Create a SKPicture (recorded drawing) from SVG
                                svg = new SKSvg();
                                svg.FromSvg(Svg);
                                // Save this SKPicture for later use
                                _bitmapId = BitmapRegistry.Instance.Register(svg.Picture);
                                _bitmapIdKey = Svg;
                                _bitmapIds.Add(Svg, _bitmapId);
                            }
                        }
                        break;
                    case PinType.Pin:
                        var skColor = Color.ToSKColor();
                        var colorInHex = $"{skColor.Red.ToString("X2")}{skColor.Green.ToString("X2")}{skColor.Blue.ToString("X2")}";
                        var text = $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"36\" height=\"56\"><path d=\"M18 .34C8.325.34.5 8.168.5 17.81c0 3.339.962 6.441 2.594 9.094H3l7.82 15.117L18 55.903l7.187-13.895L33 26.903h-.063c1.632-2.653 2.594-5.755 2.594-9.094C35.531 8.169 27.675.34 18 .34zm0 9.438a6.5 6.5 0 1 1 0 13 6.5 6.5 0 0 1 0-13z\" fill=\"#{colorInHex}\"/></svg>";
                        if (_bitmapIds.ContainsKey(colorInHex))
                        {
                            _bitmapId = _bitmapIds[colorInHex];
                            _bitmapIdKey = colorInHex;
                        }
                        else
                        {
                            svg = new SKSvg();
                            svg.FromSvg(text);
                            _bitmapId = BitmapRegistry.Instance.Register(svg.Picture);
                            _bitmapIdKey = colorInHex;
                            _bitmapIds.Add(colorInHex, _bitmapId);
                            Width = svg.Picture.CullRect.Width * Scale;
                            Height = svg.Picture.CullRect.Height * Scale;
                        }
                        break;
                    case PinType.Icon:
                        if (Icon != null)
                        {
                            using (var image = SKBitmap.Decode(Icon))
                            {
                                Width = image.Width * Scale;
                                Height = image.Height * Scale;
                                _bitmapId = BitmapRegistry.Instance.Register(new MemoryStream(Icon));
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
    }
}
