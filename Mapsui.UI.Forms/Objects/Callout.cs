﻿using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Topten.RichTextKit;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public class Callout : BindableObject, IFeatureProvider
    {
        private Pin _pin;
        private bool _updating = false;

        public event EventHandler<EventArgs> CalloutClosed;
        public event EventHandler<CalloutClickedEventArgs> CalloutClicked;

        public static string DefaultTitleFontName = Xamarin.Forms.Font.Default.FontFamily;
        public static double DefaultTitleFontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
        public static FontAttributes DefaultTitleFontAttributes = FontAttributes.Bold;
        public static Xamarin.Forms.Color DefaultTitleFontColor = Xamarin.Forms.Color.Black;
        public static Xamarin.Forms.TextAlignment DefaultTitleTextAlignment = Xamarin.Forms.TextAlignment.Center;
        public static string DefaultSubtitleFontName = Xamarin.Forms.Font.Default.FontFamily;
        public static double DefaultSubtitleFontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
        public static FontAttributes DefaultSubtitleFontAttributes = FontAttributes.None;
        public static Xamarin.Forms.Color DefaultSubtitleFontColor = Xamarin.Forms.Color.Black;
        public static Xamarin.Forms.TextAlignment DefaultSubtitleTextAlignment = Xamarin.Forms.TextAlignment.Start; // Center;

        #region Bindings

        /// <summary>
        /// Bindable property for the <see cref="Type"/> property
        /// </summary>
        public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(CalloutType), typeof(MapView), default(CalloutType));

        /// <summary>
        /// Bindable property for the <see cref="Anchor"/> property
        /// </summary>
        public static readonly BindableProperty AnchorProperty = BindableProperty.Create(nameof(Anchor), typeof(Xamarin.Forms.Point), typeof(MapView), default(Xamarin.Forms.Point));

        /// <summary>
        /// Bindable property for the <see cref="ArrowAlignment"/> property
        /// </summary>
        public static readonly BindableProperty ArrowAlignmentProperty = BindableProperty.Create(nameof(ArrowAlignment), typeof(ArrowAlignment), typeof(MapView), default(ArrowAlignment), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Bindable property for the <see cref="ArrowWidth"/> property
        /// </summary>
        public static readonly BindableProperty ArrowWidthProperty = BindableProperty.Create(nameof(ArrowWidth), typeof(double), typeof(MapView), 12.0);

        /// <summary>
        /// Bindable property for the <see cref="ArrowHeight"/> property
        /// </summary>
        public static readonly BindableProperty ArrowHeightProperty = BindableProperty.Create(nameof(ArrowHeight), typeof(double), typeof(MapView), 16.0);

        /// <summary>
        /// Bindable property for the <see cref="ArrowPosition"/> property
        /// </summary>
        public static readonly BindableProperty ArrowPositionProperty = BindableProperty.Create(nameof(ArrowPosition), typeof(double), typeof(MapView), 0.5);

        /// <summary>
        /// Bindable property for the <see cref="Color"/> property
        /// </summary>
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Xamarin.Forms.Color), typeof(MapView), Xamarin.Forms.Color.White);

        /// <summary>
        /// Bindable property for the <see cref="BackgroundColor"/> property
        /// </summary>
        public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Xamarin.Forms.Color), typeof(MapView), Xamarin.Forms.Color.White);

        /// <summary>
        /// Bindable property for the <see cref="ShadowWidth"/> property
        /// </summary>
        public static readonly BindableProperty ShadowWidthProperty = BindableProperty.Create(nameof(ShadowWidth), typeof(double), typeof(MapView), default(double));

        /// <summary>
        /// Bindable property for the <see cref="StrokeWidth"/> property
        /// </summary>
        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(double), typeof(MapView), default(double));

        /// <summary>
        /// Bindable property for the <see cref="Rotation"/> property
        /// </summary>
        public static readonly BindableProperty RotationProperty = BindableProperty.Create(nameof(Rotation), typeof(double), typeof(MapView), default(double));

        /// <summary>
        /// Bindable property for the <see cref="RotateWithMap"/> property
        /// </summary>
        public static readonly BindableProperty RotateWithMapProperty = BindableProperty.Create(nameof(RotateWithMap), typeof(bool), typeof(MapView), false);

        /// <summary>
        /// Bindable property for the <see cref="RectRadius"/> property
        /// </summary>
        public static readonly BindableProperty RectRadiusProperty = BindableProperty.Create(nameof(RectRadius), typeof(double), typeof(MapView), default(double));

        /// <summary>
        /// Bindable property for the <see cref="Padding"/> property
        /// </summary>
        public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(MapView), new Thickness(6));

        /// <summary>
        /// Bindable property for the <see cref="Spacing"/> property
        /// </summary>
        public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(MapView), 2.0);

        /// <summary>
        /// Bindable property for the <see cref="MaxWidth"/> property
        /// </summary>
        public static readonly BindableProperty MaxWidthProperty = BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(MapView), 300.0);

        /// <summary>
        /// Bindable property for the <see cref="IsClosableByClick"/> property
        /// </summary>
        public static readonly BindableProperty IsClosableByClickProperty = BindableProperty.Create(nameof(IsClosableByClick), typeof(bool), typeof(MapView), true);

        /// <summary>
        /// Bindable property for the <see cref="Content"/> property
        /// </summary>
        public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(int), typeof(MapView), -1);

        /// <summary>
        /// Bindable property for the <see cref="Title"/> property
        /// </summary>
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(MapView), default(string));

        /// <summary>
        /// Bindable property for the <see cref="TitleFontName"/> property
        /// </summary>
        public static readonly BindableProperty TitleFontNameProperty = BindableProperty.Create(nameof(TitleFontName), typeof(string), typeof(MapView), DefaultTitleFontName);

        /// <summary>
        /// Bindable property for the <see cref="TitleFontSize"/> property
        /// </summary>
        public static readonly BindableProperty TitleFontSizeProperty = BindableProperty.Create(nameof(TitleFontSize), typeof(double), typeof(MapView), DefaultTitleFontSize);

        /// <summary>
        /// Bindable property for the <see cref="TitleFontAttributes"/> property
        /// </summary>
        public static readonly BindableProperty TitleFontAttributesProperty = BindableProperty.Create(nameof(TitleFontAttributes), typeof(FontAttributes), typeof(MapView), DefaultTitleFontAttributes);

        /// <summary>
        /// Bindable property for the <see cref="TitleFontColor"/> property
        /// </summary>
        public static readonly BindableProperty TitleFontColorProperty = BindableProperty.Create(nameof(TitleFontColor), typeof(Xamarin.Forms.Color), typeof(MapView), DefaultTitleFontColor);

        /// <summary>
        /// Bindable property for the <see cref="TitleTextAlignment"/> property
        /// </summary>
        public static readonly BindableProperty TitleTextAlignmentProperty = BindableProperty.Create(nameof(TitleTextAlignment), typeof(Xamarin.Forms.TextAlignment), typeof(MapView), DefaultTitleTextAlignment);

        /// <summary>
        /// Bindable property for the <see cref="Subtitle"/> property
        /// </summary>
        public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(MapView), default(string));

        /// <summary>
        /// Bindable property for the <see cref="SubtitleFontName"/> property
        /// </summary>
        public static readonly BindableProperty SubtitleFontNameProperty = BindableProperty.Create(nameof(SubtitleFontName), typeof(string), typeof(MapView), DefaultSubtitleFontName);

        /// <summary>
        /// Bindable property for the <see cref="SubtitleFontSize"/> property
        /// </summary>
        public static readonly BindableProperty SubtitleFontSizeProperty = BindableProperty.Create(nameof(SubtitleFontSize), typeof(double), typeof(MapView), DefaultSubtitleFontSize);

        /// <summary>
        /// Bindable property for the <see cref="SubtitleFontAttributes"/> property
        /// </summary>
        public static readonly BindableProperty SubtitleFontAttributesProperty = BindableProperty.Create(nameof(SubtitleFontAttributes), typeof(FontAttributes), typeof(MapView), DefaultSubtitleFontAttributes);

        /// <summary>
        /// Bindable property for the <see cref="SubtitleFontColor"/> property
        /// </summary>
        public static readonly BindableProperty SubtitleFontColorProperty = BindableProperty.Create(nameof(SubtitleFontColor), typeof(Xamarin.Forms.Color), typeof(MapView), DefaultSubtitleFontColor);

        /// <summary>
        /// Bindable property for the <see cref="SubtitleTextAlignment"/> property
        /// </summary>
        public static readonly BindableProperty SubtitleTextAlignmentProperty = BindableProperty.Create(nameof(SubtitleTextAlignment), typeof(Xamarin.Forms.TextAlignment), typeof(MapView), DefaultSubtitleTextAlignment);

        #endregion

        public Callout(Pin pin)
        {
            if (pin == null)
            {
                throw new ArgumentNullException("Pin shouldn't be null");
            }

            _pin = pin;
            if (_pin.Feature != null)
                Feature = (Feature)_pin.Feature.Copy();
            else
                Feature = new Feature();
            Feature.Styles.Clear();
        }

        /// <summary>
        /// Pin to which this callout belongs
        /// </summary>
        public Pin Pin
        {
            get { return _pin; }
        }

        /// <summary>
        /// Type of Callout
        /// </summary>
        /// <remarks>
        /// Could be single, detail or custom. The last is a bitmap id for an owner drawn image.
        /// </remarks>
        public CalloutType Type
        {
            get { return (CalloutType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        /// <summary>
        /// Anchor position of Callout
        /// </summary>
        public Xamarin.Forms.Point Anchor
        {
            get { return (Xamarin.Forms.Point)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }

        /// <summary>
        /// Arrow alignment of Callout
        /// </summary>
        public ArrowAlignment ArrowAlignment
        {
            get { return (ArrowAlignment)GetValue(ArrowAlignmentProperty); }
            set { SetValue(ArrowAlignmentProperty, value); }
        }

        /// <summary>
        /// Width from arrow of Callout
        /// </summary>
        public double ArrowWidth
        {
            get { return (double)GetValue(ArrowWidthProperty); }
            set { SetValue(ArrowWidthProperty, value); }
        }

        /// <summary>
        /// Height from arrow of Callout
        /// </summary>
        public double ArrowHeight
        {
            get { return (double)GetValue(ArrowHeightProperty); }
            set { SetValue(ArrowHeightProperty, value); }
        }

        /// <summary>
        /// Relative position of anchor of Callout on the side given by <see cref="ArrowAlignment">
        /// </summary>
        public double ArrowPosition
        {
            get { return (double)GetValue(ArrowPositionProperty); }
            set { SetValue(ArrowPositionProperty, value); }
        }

        /// <summary>
        /// Color of stroke around Callout
        /// </summary>
        public Xamarin.Forms.Color Color
        {
            get { return (Xamarin.Forms.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// BackgroundColor of Callout
        /// </summary>
        public Xamarin.Forms.Color BackgroundColor
        {
            get { return (Xamarin.Forms.Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        /// <summary>
        /// Shadow width around Callout
        /// </summary>
        public double ShadowWidth
        {
            get { return (double)GetValue(ShadowWidthProperty); }
            set { SetValue(ShadowWidthProperty, value); }
        }

        /// <summary>
        /// Stroke width of frame around Callout
        /// </summary>
        public double StrokeWidth
        {
            get { return (double)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        /// <summary>
        /// Rotation of Callout around the anchor
        /// </summary>
        public double Rotation
        {
            get { return (double)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }

        /// <summary>
        /// Rotate Callout with map
        /// </summary>
        public bool RotateWithMap
        {
            get { return (bool)GetValue(RotateWithMapProperty); }
            set { SetValue(RotateWithMapProperty, value); }
        }

        /// <summary>
        /// Radius of rounded corners of Callout
        /// </summary>
        public double RectRadius
        {
            get { return (double)GetValue(RectRadiusProperty); }
            set { SetValue(RectRadiusProperty, value); }
        }

        /// <summary>
        /// Padding around content of Callout
        /// </summary>
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Space between Title and Subtitel of Callout
        /// </summary>
        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        /// <summary>
        /// MaxWidth for Title and Subtitel of Callout
        /// </summary>
        public double MaxWidth
        {
            get { return (double)GetValue(MaxWidthProperty); }
            set { SetValue(MaxWidthProperty, value); }
        }

        /// <summary>
        /// Is Callout visible on map
        /// </summary>
        public bool IsVisible
        {
            get { return _pin.IsCalloutVisible(); }
        }

        /// <summary>
        /// Is Callout closable by a click on the callout
        /// </summary>
        public bool IsClosableByClick
        {
            get { return (bool)GetValue(IsClosableByClickProperty); }
            private set { SetValue(IsClosableByClickProperty, value); }
        }

        /// <summary>
        /// Content of Callout
        /// </summary>
        public int Content
        {
            get { return (int)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Content of Callout title label
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Font name to use rendering title
        /// </summary>
        public string TitleFontName
        {
            get { return (string)GetValue(TitleFontNameProperty); }
            set { SetValue(TitleFontNameProperty, value); }
        }

        /// <summary>
        /// Font size to rendering title
        /// </summary>
        public double TitleFontSize
        {
            get { return (double)GetValue(TitleFontSizeProperty); }
            set { SetValue(TitleFontSizeProperty, value); }
        }

        /// <summary>
        /// Font attributes to render title
        /// </summary>
        public FontAttributes TitleFontAttributes
        {
            get { return (FontAttributes)GetValue(TitleFontAttributesProperty); }
            set { SetValue(TitleFontAttributesProperty, value); }
        }

        /// <summary>
        /// Font color to render title
        /// </summary>
        public Xamarin.Forms.Color TitleFontColor
        {
            get { return (Xamarin.Forms.Color)GetValue(TitleFontColorProperty); }
            set { SetValue(TitleFontColorProperty, value); }
        }

        /// <summary>
        /// Text alignment of title
        /// </summary>
        public Xamarin.Forms.TextAlignment TitleTextAlignment
        {
            get { return (Xamarin.Forms.TextAlignment)GetValue(TitleTextAlignmentProperty); }
            set { SetValue(TitleTextAlignmentProperty, value); }
        }

        /// <summary>
        /// Content of Callout detail label
        /// </summary>
        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        /// <summary>
        /// Font name to use rendering subtitle
        /// </summary>
        public string SubtitleFontName
        {
            get { return (string)GetValue(SubtitleFontNameProperty); }
            set { SetValue(SubtitleFontNameProperty, value); }
        }

        /// <summary>
        /// Font size to rendering subtitle
        /// </summary>
        public double SubtitleFontSize
        {
            get { return (double)GetValue(SubtitleFontSizeProperty); }
            set { SetValue(SubtitleFontSizeProperty, value); }
        }

        /// <summary>
        /// Font attributes to render subtitle
        /// </summary>
        public FontAttributes SubtitleFontAttributes
        {
            get { return (FontAttributes)GetValue(SubtitleFontAttributesProperty); }
            set { SetValue(SubtitleFontAttributesProperty, value); }
        }

        /// <summary>
        /// Font color to render subtitle
        /// </summary>
        public Xamarin.Forms.Color SubtitleFontColor
        {
            get { return (Xamarin.Forms.Color)GetValue(SubtitleFontColorProperty); }
            set { SetValue(SubtitleFontColorProperty, value); }
        }

        /// <summary>
        /// Text alignment of title
        /// </summary>
        public Xamarin.Forms.TextAlignment SubtitleTextAlignment
        {
            get { return (Xamarin.Forms.TextAlignment)GetValue(SubtitleTextAlignmentProperty); }
            set { SetValue(SubtitleTextAlignmentProperty, value); }
        }

        /// <summary>
        /// Feature, which belongs to callout. Should be the same as for the pin to which this callout belongs.
        /// </summary>
        public Feature Feature { get; }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);

            if (Type != CalloutType.Custom && propertyName.Equals(nameof(Content)))
            {
                Type = CalloutType.Custom;
            }

            if (IsVisible && (propertyName.Equals(nameof(Title))
                || propertyName.Equals(nameof(Subtitle))
                || propertyName.Equals(nameof(Content))
                || propertyName.Equals(nameof(Type))
                || propertyName.Equals(nameof(TitleFontName))
                || propertyName.Equals(nameof(TitleFontSize))
                || propertyName.Equals(nameof(TitleFontAttributes))
                || propertyName.Equals(nameof(TitleFontColor))
                || propertyName.Equals(nameof(TitleTextAlignment))
                || propertyName.Equals(nameof(SubtitleFontName))
                || propertyName.Equals(nameof(SubtitleFontSize))
                || propertyName.Equals(nameof(SubtitleFontAttributes))
                || propertyName.Equals(nameof(SubtitleFontColor))
                || propertyName.Equals(nameof(SubtitleTextAlignment))
                || propertyName.Equals(nameof(Spacing))
                || propertyName.Equals(nameof(MaxWidth)))
                )
            {
                UpdateContent();
            }
            else if (IsVisible && propertyName.Equals(nameof(ArrowAlignment))
                || propertyName.Equals(nameof(ArrowWidth))
                || propertyName.Equals(nameof(ArrowHeight))
                || propertyName.Equals(nameof(ArrowPosition))
                || propertyName.Equals(nameof(Anchor))
                || propertyName.Equals(nameof(IsVisible))
                || propertyName.Equals(nameof(Padding))
                || propertyName.Equals(nameof(Color))
                || propertyName.Equals(nameof(BackgroundColor))
                || propertyName.Equals(nameof(RectRadius)))
            {
                UpdateCalloutStyle();
            }

            _pin?.MapView?.Refresh();
        }

        /// <summary>
        /// Callout is touched
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">CalloutClickedEventArgs</param>
        internal void HandleCalloutClicked(object sender, CalloutClickedEventArgs e)
        {
            CalloutClicked?.Invoke(this, e);

            if (e.Handled)
                return;

            // Check, if callout is closeable by click
            if (IsClosableByClick)
            {
                _pin.HideCallout();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Checks type of Callout and activates correct content
        /// </summary>
        private void UpdateContent()
        {
            CalloutStyle style = (CalloutStyle)Feature.Styles.Where((s) => s is CalloutStyle).FirstOrDefault();

            if (style == null)
            {
                style = new CalloutStyle();
                Feature.Styles.Add(style);
            }

            style.Type = Type;
            style.Content = Content;
            style.Title = Title;
            style.TitleFont.FontFamily = TitleFontName;
            style.TitleFont.Size = TitleFontSize;
            style.TitleFont.Italic = (TitleFontAttributes & FontAttributes.Italic) != 0;
            style.TitleFont.Bold = (TitleFontAttributes & FontAttributes.Bold) != 0;
            style.TitleFontColor = TitleFontColor.ToMapsui();
            style.TitleTextAlignment = TitleTextAlignment.ToMapsui();
            style.Subtitle = Subtitle;
            style.SubtitleFont.FontFamily = SubtitleFontName;
            style.SubtitleFont.Size = SubtitleFontSize;
            style.SubtitleFont.Italic = (SubtitleFontAttributes & FontAttributes.Italic) != 0;
            style.SubtitleFont.Bold = (SubtitleFontAttributes & FontAttributes.Bold) != 0;
            style.SubtitleFontColor = SubtitleFontColor.ToMapsui();
            style.SubtitleTextAlignment = SubtitleTextAlignment.ToMapsui();
            style.Spacing = Spacing;
            style.MaxWidth = MaxWidth;
        }

        /// <summary>
        /// Update CalloutStyle of Feature
        /// </summary>
        private void UpdateCalloutStyle()
        {
            CalloutStyle style = (CalloutStyle)Feature.Styles.Where((s) => s is CalloutStyle).FirstOrDefault();

            if (style == null)
            {
                style = new CalloutStyle();
                Feature.Styles.Add(style);
            }

            style.ArrowAlignment = ArrowAlignment;
            style.ArrowHeight = (float)ArrowHeight;
            style.ArrowPosition = (float)ArrowPosition;
            style.BackgroundColor = BackgroundColor.ToMapsui(); ;
            style.Color = Color.ToMapsui();
            style.SymbolOffset = new Offset(Anchor.X, Anchor.Y);
            style.Padding = new BoundingBox(Padding.Left, Padding.Top, Padding.Right, Padding.Bottom);
            style.RectRadius = (float)RectRadius;
            style.RotateWithMap = RotateWithMap;
            style.Rotation = (float)Rotation;
            style.ShadowWidth = (float)ShadowWidth;
            style.StrokeWidth = (float)StrokeWidth;
            style.Content = Content;
        }

        /// <summary>
        /// Update content and style of callout before display it the first time
        /// </summary>
        internal void Update()
        {
            UpdateContent();
            UpdateCalloutStyle();
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
