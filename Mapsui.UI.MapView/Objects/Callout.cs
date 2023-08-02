using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Extensions;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Objects;
#if __MAUI__
using Mapsui.UI.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;
using Point = Microsoft.Maui.Graphics.Point;
#else
using Mapsui.UI.Forms.Extensions;
using Xamarin.Forms;
using CalloutStyle = Mapsui.Styles.CalloutStyle;

using Color = Xamarin.Forms.Color;
using KnownColor = Xamarin.Forms.Color;
using Point = Xamarin.Forms.Point;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

public class Callout : IFeatureProvider, IDisposable, INotifyPropertyChanged
{
    private readonly Pin _pin;

    public event EventHandler<CalloutClickedEventArgs>? CalloutClicked;

    public static double DefaultTitleFontSize = 24;
    public static FontAttributes DefaultTitleFontAttributes = FontAttributes.Bold;
    public static TextAlignment DefaultTitleTextAlignment = TextAlignment.Center;
    public static Color DefaultTitleFontColor = KnownColor.Black;
    public static double DefaultSubtitleFontSize = 20;
    public static FontAttributes DefaultSubtitleFontAttributes = FontAttributes.None;
    public static Color DefaultSubtitleFontColor = KnownColor.Black;
    public static TextAlignment DefaultSubtitleTextAlignment = TextAlignment.Start; // Center;
    public static string? DefaultTitleFontName = null;
    public static string? DefaultSubtitleFontName = null;

    private CalloutType _type;
    private Point _anchor;
    private ArrowAlignment _arrowAlignment;
    private double _arrowWidth = 12.0;
    private double _arrowHeight = 16.0;
    private double _arrowPosition = 0.5;
    private Color _color = KnownColor.White;
    private Color _backgroundColor = KnownColor.White;
    private double _shadowWidth;
    private double _strokeWidth;
    private double _rotation;
    private bool _rotateWithMap;
    private double _rectRadius;
    private Thickness _padding = new Thickness(6);
    private double _spacing = 2;
    private double _maxWidth = 300.0;
    private bool _isClosableByClick = true;
    private int _content = -1;
    private string? _title;
    private string? _titleFontName = DefaultTitleFontName;
    private double _titleFontSize = DefaultTitleFontSize;
    private FontAttributes _titleFontAttributes = DefaultTitleFontAttributes;
    private Color _titleFontColor = DefaultTitleFontColor;
    private TextAlignment _titleTextAlignment = DefaultTitleTextAlignment;
    private string? _subtitle;
    private string? _subtitleFontName = DefaultSubtitleFontName;
    private double _subtitleFontSize = DefaultSubtitleFontSize;
    private FontAttributes _subtitleFontAttributes = DefaultSubtitleFontAttributes;
    private Color _subtitleFontColor = DefaultSubtitleFontColor;
    private TextAlignment _subtitleTextAlignment = DefaultSubtitleTextAlignment;

    public Callout(Pin pin)
    {
        _pin = pin ?? throw new ArgumentNullException(nameof(pin), "Pin shouldn't be null");
        if (_pin.Feature != null)
            Feature = _pin.Feature.Copy();
        else
            Feature = new GeometryFeature();
        Feature.Styles.Clear();
    }

    /// <summary>
    /// Pin to which this callout belongs
    /// </summary>
    public Pin Pin => _pin;

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
            if (value == _type) return;
            _type = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anchor position of Callout
    /// </summary>
    public Point Anchor
    {
        get => _anchor;
        set
        {
            if (value.Equals(_anchor)) return;
            _anchor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Arrow alignment of Callout
    /// </summary>
    public ArrowAlignment ArrowAlignment
    {
        get => _arrowAlignment;
        set
        {
            if (value == _arrowAlignment) return;
            _arrowAlignment = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Width from arrow of Callout
    /// </summary>
    public double ArrowWidth
    {
        get => _arrowWidth;
        set
        {
            if (value.Equals(_arrowWidth)) return;
            _arrowWidth = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Height from arrow of Callout
    /// </summary>
    public double ArrowHeight
    {
        get => _arrowHeight;
        set
        {
            if (value.Equals(_arrowHeight)) return;
            _arrowHeight = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Relative position of anchor of Callout on the side given by <see cref="ArrowAlignment"/>
    /// </summary>
    public double ArrowPosition
    {
        get => _arrowPosition;
        set
        {
            if (value.Equals(_arrowPosition)) return;
            _arrowPosition = value;
            OnPropertyChanged();
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
            if (value.Equals(_color)) return;
            _color = value;
            OnPropertyChanged();
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
            if (value.Equals(_backgroundColor)) return;
            _backgroundColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Shadow width around Callout
    /// </summary>
    public double ShadowWidth
    {
        get => _shadowWidth;
        set
        {
            if (value.Equals(_shadowWidth)) return;
            _shadowWidth = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Stroke width of frame around Callout
    /// </summary>
    public double StrokeWidth
    {
        get => _strokeWidth;
        set
        {
            if (value.Equals(_strokeWidth)) return;
            _strokeWidth = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Rotation of Callout around the anchor
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        set
        {
            if (value.Equals(_rotation)) return;
            _rotation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Rotate Callout with map
    /// </summary>
    public bool RotateWithMap
    {
        get => _rotateWithMap;
        set
        {
            if (value == _rotateWithMap) return;
            _rotateWithMap = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Radius of rounded corners of Callout
    /// </summary>
    public double RectRadius
    {
        get => _rectRadius;
        set
        {
            if (value.Equals(_rectRadius)) return;
            _rectRadius = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Padding around content of Callout
    /// </summary>
    public Thickness Padding
    {
        get => _padding;
        set
        {
            if (value.Equals(_padding)) return;
            _padding = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Space between Title and Subtitle of Callout
    /// </summary>
    public double Spacing
    {
        get => _spacing;
        set
        {
            if (value.Equals(_spacing)) return;
            _spacing = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// MaxWidth for Title and Subtitle of Callout
    /// </summary>
    public double MaxWidth
    {
        get => _maxWidth;
        set
        {
            if (value.Equals(_maxWidth)) return;
            _maxWidth = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Is Callout visible on map
    /// </summary>
    public bool IsVisible => _pin.IsCalloutVisible();

    /// <summary>
    /// Is Callout closable by a click on the callout
    /// </summary>
    public bool IsClosableByClick
    {
        get => _isClosableByClick;
        set
        {
            if (value == _isClosableByClick) return;
            _isClosableByClick = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Content of Callout
    /// </summary>
    public int Content
    {
        get => _content;
        set
        {
            if (value == _content) return;
            _content = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Content of Callout title label
    /// </summary>
    public string? Title
    {
        get => _title;
        set
        {
            if (value == _title) return;
            _title = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font name to use rendering title
    /// </summary>
    public string? TitleFontName
    {
        get => _titleFontName;
        set
        {
            if (value == _titleFontName) return;
            _titleFontName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font size to rendering title
    /// </summary>
    public double TitleFontSize
    {
        get => _titleFontSize;
        set
        {
            if (value.Equals(_titleFontSize)) return;
            _titleFontSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font attributes to render title
    /// </summary>
    public FontAttributes TitleFontAttributes
    {
        get => _titleFontAttributes;
        set
        {
            if (value == _titleFontAttributes) return;
            _titleFontAttributes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font color to render title
    /// </summary>
    public Color TitleFontColor
    {
        get => _titleFontColor;
        set
        {
            if (value.Equals(_titleFontColor)) return;
            _titleFontColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Text alignment of title
    /// </summary>
    public TextAlignment TitleTextAlignment
    {
        get => _titleTextAlignment;
        set
        {
            if (value == _titleTextAlignment) return;
            _titleTextAlignment = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Content of Callout detail label
    /// </summary>
    public string? Subtitle
    {
        get => _subtitle;
        set
        {
            if (value == _subtitle) return;
            _subtitle = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font name to use rendering subtitle
    /// </summary>
    public string? SubtitleFontName
    {
        get => _subtitleFontName;
        set
        {
            if (value == _subtitleFontName) return;
            _subtitleFontName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font size to rendering subtitle
    /// </summary>
    public double SubtitleFontSize
    {
        get => _subtitleFontSize;
        set
        {
            if (value.Equals(_subtitleFontSize)) return;
            _subtitleFontSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font attributes to render subtitle
    /// </summary>
    public FontAttributes SubtitleFontAttributes
    {
        get => _subtitleFontAttributes;
        set
        {
            if (value == _subtitleFontAttributes) return;
            _subtitleFontAttributes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Font color to render subtitle
    /// </summary>
    public Color SubtitleFontColor
    {
        get => _subtitleFontColor;
        set
        {
            if (value.Equals(_subtitleFontColor)) return;
            _subtitleFontColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Text alignment of title
    /// </summary>
    public TextAlignment SubtitleTextAlignment
    {
        get => _subtitleTextAlignment;
        set
        {
            if (value == _subtitleTextAlignment) return;
            _subtitleTextAlignment = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Feature, which belongs to callout. Should be the same as for the pin to which this callout belongs.
    /// </summary>
    public GeometryFeature Feature { get; }

    /// <summary>
    /// Callout is touched
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">CalloutClickedEventArgs</param>
    internal void HandleCalloutClicked(object? sender, CalloutClickedEventArgs e)
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
        var style = Feature.Styles.Where((s) => s is CalloutStyle).FirstOrDefault() as CalloutStyle;

        if (style is null)
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
        var style = Feature.Styles.FirstOrDefault(s => s is CalloutStyle) as CalloutStyle;

        if (style is null)
        {
            style = new CalloutStyle();
            Feature.Styles.Add(style);
        }

        style.ArrowAlignment = ArrowAlignment;
        style.ArrowHeight = (float)ArrowHeight;
        style.ArrowPosition = (float)ArrowPosition;
        style.BackgroundColor = BackgroundColor.ToMapsui();
        style.Color = Color.ToMapsui();
        style.SymbolOffset = new Offset(Anchor.X, Anchor.Y);
        style.SymbolOffsetRotatesWithMap = _pin.RotateWithMap;
        style.Padding = new MRect(Padding.Left, Padding.Top, Padding.Right, Padding.Bottom);
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

    public virtual void Dispose()
    {
        Feature.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName == null)
            return;

        if (Type != CalloutType.Custom && propertyName.Equals(nameof(Content)))
            Type = CalloutType.Custom;

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
            UpdateContent();
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
            UpdateCalloutStyle();

        _pin.MapView?.Refresh();
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
