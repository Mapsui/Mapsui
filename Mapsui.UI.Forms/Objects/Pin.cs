using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public sealed class Pin : BindableObject, IFeatureProvider
    {
        private int DefaultPin = -1;

        public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(PinType), typeof(Pin), default(PinType));

        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Xamarin.Forms.Color), typeof(Pin), SKColors.Red.ToFormsColor());

        public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(Position), typeof(Pin), default(Position));

        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));

        public static readonly BindableProperty AddressProperty = BindableProperty.Create(nameof(Address), typeof(string), typeof(Pin), default(string));

        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(byte[]), typeof(Pin), default(byte[]));

        public static readonly BindableProperty RotationProperty = BindableProperty.Create(nameof(Rotation), typeof(float), typeof(Pin), 0f);

        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(Pin), true);

        public static readonly BindableProperty AnchorProperty = BindableProperty.Create(nameof(Anchor), typeof(Point), typeof(Pin), new Point(0, 28));

        public static readonly BindableProperty InfoWindowAnchorProperty = BindableProperty.Create(nameof(InfoWindowAnchor), typeof(Point), typeof(Pin), new Point(0.5d, 1.0d));

        public static readonly BindableProperty TransparencyProperty = BindableProperty.Create(nameof(Transparency), typeof(float), typeof(Pin), 0f);

        public Pin()
        {
            CreateFeature();
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

        private int iconId = -1;

        /// <summary>
        /// Byte[] holding the bitmap informations
        /// </summary>
        public byte[] Icon
        {
            get { return (byte[])GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
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
        /// Determins, if the pin is drawn on map
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// Anchor of the bitmap in pixel
        /// </summary>
        public Point Anchor
        {
            get { return (Point)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }

        public Point InfoWindowAnchor
        {
            get { return (Point)GetValue(InfoWindowAnchorProperty); }
            set { SetValue(InfoWindowAnchorProperty, value); }
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

        private Feature feature;

        public Feature Feature
        {
            get
            {
                return feature;
            }
            set
            {
                if (feature == null || !feature.Equals(value))
                    feature = value;
            }
        }

        private byte[] PinIcon;

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
                    feature.Geometry = Position.ToMapsui();
                    break;
                case nameof(Label):
                    feature["Label"] = Label;
                    break;
                case nameof(Type):
                case nameof(Anchor):
                case nameof(Rotation):
                case nameof(Transparency):
                case nameof(Color):
                    CreateFeature();
                    break;
                case nameof(Icon):
                    if (iconId != -1)
                    {
                        BitmapRegistry.Instance.Unregister(iconId);
                        iconId = -1;
                    }
                    CreateFeature();
                    break;
            }
        }

        private object sync = new object();

        private void CreateFeature()
        {
            lock (sync)
            {
                // Create a new one
                var f = new Feature
                {
                    Geometry = Position.ToMapsui(),
                    ["Label"] = Label,
                };
                // Check for bitmapId
                var bitmapId = iconId;
                if (Type == PinType.Pin)
                {
                    if (DefaultPin != -1)
                        BitmapRegistry.Instance.Unregister(DefaultPin);
                    // Create a new SVG object
                    var svg = new SkiaSharp.Extended.Svg.SKSvg();
                    var assembly = typeof(Pin).GetTypeInfo().Assembly;
                    // Load the SVG document
                    svg.Load(assembly.GetManifestResourceStream($"Mapsui.UI.Images.Pin.svg"));
                    // Create bitmap to hold canvas
                    var info = new SKImageInfo((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height) { AlphaType = SKAlphaType.Premul };
                    var bitmap = new SKBitmap(info);
                    var canvas = new SKCanvas(bitmap);
                    // Replace color while drawing
                    using (var paint = new SKPaint())
                    {
                        paint.ColorFilter = SKColorFilter.CreateBlendMode(
                            Color.ToSKColor(),  
                            SKBlendMode.SrcIn); // use the source color
                        canvas.Clear();
                        canvas.DrawPicture(svg.Picture, paint);
                    }
                    // Now convert canvas to bitmap
                    using (var image = SKImage.FromBitmap(bitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        PinIcon = data.ToArray();
                    }
                    DefaultPin = BitmapRegistry.Instance.Register(new MemoryStream(PinIcon));
                    bitmapId = DefaultPin;
                }
                if (Type == PinType.Icon)
                {
                    if (iconId < 0 && Icon != null)
                        iconId = BitmapRegistry.Instance.Register(new MemoryStream(Icon));
                    bitmapId = iconId;
                }
                if (bitmapId != -1)
                {
                    f.Styles.Add(new SymbolStyle
                    {
                        BitmapId = bitmapId,
                        SymbolScale = 0.8,
                        SymbolRotation = Rotation,
                        SymbolOffset = new Offset(Anchor.X, Anchor.Y),
                        Opacity = 1 - Transparency,
                    });
                }
                else
                {
                    f.Styles.Add(new Styles.SymbolStyle()
                    {
                        Fill = new Brush { Color = Styles.Color.Red },
                        Line = new Pen { Color = Styles.Color.Black, Width = 2 },
                        SymbolScale = 0.8,
                        SymbolRotation = Rotation,
                        Opacity = 1 - Transparency,
                        SymbolOffset = new Offset(Anchor.X, Anchor.Y),
                    });
                }
                feature = f;
                System.Diagnostics.Debug.WriteLine("Feature created");
            }
        }

        private List<Styles.Style> CreatePinFeature()
        {
            var styles = new List<Styles.Style>();

            styles.Add(new SymbolStyle
            {
                Fill = new Brush { Color = Styles.Color.Red },
                Line = new Pen { Color = Styles.Color.Black, Width = 2 },
                SymbolRotation = Rotation,
                Opacity = Transparency,
                SymbolOffset = new Offset(Anchor.X, Anchor.Y),

            });

            return styles;
        }
    }
}