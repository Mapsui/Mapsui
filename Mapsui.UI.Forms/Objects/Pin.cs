using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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
        private int bitmapId = -1;
        private byte[] bitmapData;

        public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(PinType), typeof(Pin), default(PinType));
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Xamarin.Forms.Color), typeof(Pin), SKColors.Red.ToFormsColor());
        public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(Position), typeof(Pin), default(Position));
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));
        public static readonly BindableProperty AddressProperty = BindableProperty.Create(nameof(Address), typeof(string), typeof(Pin), default(string));
        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(byte[]), typeof(Pin), default(byte[]));
        public static readonly BindableProperty SvgProperty = BindableProperty.Create(nameof(Svg), typeof(string), typeof(Pin), default(string));
        public static readonly BindableProperty ScaleProperty = BindableProperty.Create(nameof(Scale), typeof(float), typeof(Pin), 1.0f);
        public static readonly BindableProperty RotationProperty = BindableProperty.Create(nameof(Rotation), typeof(float), typeof(Pin), 0f);
        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(Pin), true);
        public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(double), typeof(Pin), -1.0, BindingMode.OneWayToSource);
        public static readonly BindableProperty HeightProperty = BindableProperty.Create(nameof(Height), typeof(double), typeof(Pin), -1.0);
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
        /// Scaling of pin, place where anchor is
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
        /// Determins, if the pin is drawn on map
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
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
        }

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
                case nameof(Transparency):
                    ((SymbolStyle)feature.Styles.First()).Opacity = 1 - Transparency;
                    break;
                case nameof(Anchor):
                    ((SymbolStyle)feature.Styles.First()).SymbolOffset = new Offset(Anchor.X, Anchor.Y);
                    break;
                case nameof(Rotation):
                    ((SymbolStyle)feature.Styles.First()).SymbolRotation = Rotation;
                    break;
                case nameof(IsVisible):
                    ((SymbolStyle)feature.Styles.First()).Enabled = IsVisible;
                    break;
                case nameof(Scale):
                    ((SymbolStyle)feature.Styles.First()).SymbolScale = Scale;
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

        private object sync = new object();

        private void CreateFeature()
        {
            lock (sync)
            {
                if (feature == null)
                {
                    // Create a new one
                    feature = new Feature
                    {
                        Geometry = Position.ToMapsui(),
                        ["Label"] = Label,
                    };
                }
                // Check for bitmapId
                if (bitmapId != -1)
                {
                    // There is already a registered bitmap, so delete it
                    BitmapRegistry.Instance.Unregister(bitmapId);
                }

                // We don't have any bitmap up to now
                bitmapId = -1;

                switch (Type)
                {
                    case PinType.Pin:
                    case PinType.Svg:
                        // First we have to create a bitmap from Svg code
                        // Create a new SVG object
                        var svg = new SkiaSharp.Extended.Svg.SKSvg();
                        var assembly = typeof(Pin).GetTypeInfo().Assembly;
                        // Load the SVG document
                        Stream stream = null;
                        if (Type == PinType.Pin)
                            stream = assembly.GetManifestResourceStream($"Mapsui.UI.Images.Pin.svg");
                        else
                            if (!string.IsNullOrEmpty(Svg))
                                stream = new MemoryStream(Encoding.UTF8.GetBytes(Svg));
                        if (stream == null)
                            return;
                        svg.Load(stream);
                        Width = svg.CanvasSize.Width * Scale;
                        Height = svg.CanvasSize.Height * Scale;
                        // Create bitmap to hold canvas
                        var info = new SKImageInfo((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height) { AlphaType = SKAlphaType.Premul };
                        var bitmap = new SKBitmap(info);
                        var canvas = new SKCanvas(bitmap);
                        // Now draw Svg image to bitmap
                        using (var paint = new SKPaint())
                        {
                            if (Type == PinType.Pin)
                                // Replace color while drawing
                                paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn); // use the source color
                            canvas.Clear();
                            canvas.DrawPicture(svg.Picture, paint);
                        }
                        // Now convert canvas to bitmap
                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            bitmapData = data.ToArray();
                        }
                        bitmapId = BitmapRegistry.Instance.Register(new MemoryStream(bitmapData));
                        break;
                    case PinType.Icon:
                        if (Icon != null)
                        {
                            using (var image = SKBitmap.Decode(Icon))
                            {
                                Width = image.Width * Scale;
                                Height = image.Height * Scale;
                                bitmapId = BitmapRegistry.Instance.Register(new MemoryStream(Icon));
                            }
                        }
                        break;
                }
                // If we have a bitmapId (and we should have one), than draw bitmap, otherwise nothing
                if (bitmapId != -1)
                {
                    // We only want to have one style
                    feature.Styles.Clear();
                    feature.Styles.Add(new SymbolStyle
                    {
                        BitmapId = bitmapId,
                        SymbolScale = Scale,
                        SymbolRotation = Rotation,
                        SymbolOffset = new Offset(Anchor.X, Anchor.Y),
                        Opacity = 1 - Transparency,
                    });
                }
            }
        }
    }
}