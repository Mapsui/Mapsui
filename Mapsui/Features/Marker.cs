using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Color = Mapsui.Styles.Color;

namespace Mapsui.Features;

internal class Marker : PointFeature
{
    private readonly static Dictionary<MarkerType, string> _defaultPins = new();

    private readonly SymbolStyle _style = new SymbolStyle();
    private readonly CalloutStyle _calloutStyle = new CalloutStyle();

    public Marker(double x, double y, MarkerType type = MarkerType.Pin) : base(x, y)
    {
        InitMarker(type);
    }

    public Marker((double x, double y) position, MarkerType type = MarkerType.Pin) : base(position.x, position.y)
    {
        InitMarker(type);
    }

    public Marker(MPoint point, MarkerType type = MarkerType.Pin) : base(point)
    {
        InitMarker(type);
    }

    public Marker(PointFeature pointFeature, MarkerType type = MarkerType.Pin) : base(pointFeature)
    {
        InitMarker(type);
    }

    private MarkerType _markerType = MarkerType.Pin;

    /// <summary>
    /// Type of marker
    /// </summary>
    public MarkerType MarkerType 
    { 
        get => _markerType;
        set
        {
            if (_markerType == value) return;
            _markerType = value;
            UpdateMarker();
        }
    }

    private byte[]? _icon = null;

    /// <summary>
    /// Byte[] holding the bitmap informations
    /// </summary>
    public byte[]? Icon
    {
        get => _icon;
        set
        {
            if (Equals(value, _icon)) return;
            _icon = value;
            UpdateMarker();
        }
    }

    private string? _svg = string.Empty;

    /// <summary>
    /// String holding the Svg image informations
    /// </summary>
    public string? Svg
    {
        get => _svg;
        set
        {
            if (value == _svg) return;
            _svg = value;
            UpdateMarker();
        }
    }

    private double _scale = 1.0;

    /// <summary>
    /// Scaling of marker
    /// </summary>
    public double Scale
    {
        get => _scale;
        set
        {
            if (value.Equals(_scale)) return;
            _scale = value;
            _style.SymbolScale = _scale;
        }
    }

    // Default offsets for all other markers except pin (center/center)
    private static Offset _defaultAnchor = new Offset(0.0, 0.0);
    // Default offsets for all pin markers (center/bottom)
    private static Offset _defaultPinAnchor = new Offset(0.0, 0.5, true);

    private Offset _anchor = new Offset(0.0, 0.0);

    /// <summary>
    /// Anchor of bitmap in pixel
    /// </summary>
    public Offset Anchor
    {
        get => _anchor;
        set
        {
            if (value.Equals(_anchor)) return;
            _anchor = value;
            _style.SymbolOffset = _anchor;
        }
    }

    private Color _color = Color.Red;
    
    /// <summary>
    /// Color for pin (not used for Icon or Svg)
    /// </summary>
    public Color Color
    { 
        get => _color;
        set
        {
            if (_color == value)
                return;
            _color = value;
            UpdateMarker();
        }
    }

    public string? Title
    {
        get => _calloutStyle.Title;
        set 
        { 
            if (_calloutStyle.Title == value) 
                return;
            _calloutStyle.Title = value;
        }
    }
    /// <summary>
    /// True if the callout is visible
    /// </summary>
    public bool HasCallout => _calloutStyle.Enabled;

    // Default offsets for callout for all other markers except pin (center/top)
    private static Offset _defaultCalloutOffset = new Offset(0.0, 0.5, true);
    // Default offsets for callout for all pin markers (center/top too, but anchor of pin is center/bottom not center/center)
    private static Offset _defaultPinCalloutOffset = new Offset(0.0, 1.0, true);

    /// <summary>
    /// Show callout with <c ref="Title" /> as text
    /// </summary>
    public void ShowCallout()
    {
        if (_calloutStyle == null) return;

        _calloutStyle.Enabled = true;
    }

    public void HideCallout()
    {
        if (_calloutStyle == null) return;

        _calloutStyle.Enabled = false;
    }

    /// <summary>
    /// Initialize marker
    /// </summary>
    /// <param name="type">Type of marker</param>
    private void InitMarker(MarkerType type)
    {
        AssertDefaultPins();

        // Remove all existing styles
        Styles.Clear();
        
        // Set default values for style
        _style.Enabled = true;
        _style.SymbolType = SymbolType.Image;
        _style.SymbolOffset = _defaultPinAnchor;
        _style.SymbolScale = _scale;

        UpdateMarker();

        // Add style to Styles for this feature
        Styles.Add(_style);

        // Set default values for callout style
        _calloutStyle.BelongsTo = _style;
        _calloutStyle.Enabled = false;
        _calloutStyle.Type = CalloutType.Single;
        _calloutStyle.ArrowPosition = 0.5f;
        _calloutStyle.ArrowAlignment = ArrowAlignment.Bottom;
        _calloutStyle.SymbolOffset = _defaultCalloutOffset;
        _calloutStyle.Padding = new MRect(10, 5, 10, 5);
        _calloutStyle.Color = Color.Black;
        _calloutStyle.BackgroundColor = Color.White;
        _calloutStyle.MaxWidth = 200;
        _calloutStyle.TitleFontColor = Color.Black;
        _calloutStyle.TitleFont.Size = 16;
        _calloutStyle.TitleTextAlignment = Widgets.Alignment.Center;

        // Add CalloutStyle for this feature
        Styles.Add(_calloutStyle);

        // TODO: Remove when ready, only for test
        //Styles.Add(new SymbolStyle { SymbolType = SymbolType.Ellipse, SymbolScale = 0.3, Outline = new Pen(Color.Red, 5), Fill = new Brush(Color.Transparent) });
    }

    private void UpdateMarker()
    {
        // TODO: Don't do this, because BitmapRegistry doesn't count the number of usages of a bitmap
        // BitmapRegistry.Instance.Unregister(_style.BitmapId);
        // _style.BitmapId = -1;

        switch (_markerType)
        {
            case MarkerType.Icon:
                if (Icon == null) return;
                _style.BitmapId = BitmapRegistry.Instance.Register(Icon);
                _style.SymbolOffset = _defaultAnchor;
                _calloutStyle.SymbolOffset = _defaultCalloutOffset;
                return;
            case MarkerType.Svg:
                if (string.IsNullOrEmpty(Svg)) return;
                _style.BitmapId = BitmapRegistry.Instance.Register(Svg);
                _style.SymbolOffset = _defaultAnchor;
                _calloutStyle.SymbolOffset = _defaultCalloutOffset;
                return;
            default:
                _style.BitmapId = GetPinWithColor();
                _style.SymbolOffset = _defaultPinAnchor;
                _calloutStyle.SymbolOffset = _defaultPinCalloutOffset;
                return;
        }
    }

    private int GetPinWithColor()
    {
        if (MarkerType == MarkerType.Icon || MarkerType == MarkerType.Svg)
            return -1;

        var colorInHex = $"{Color.R:X2}{Color.G:X2}{Color.B:X2}";
        var pinName = MarkerType.ToString();

        if (BitmapRegistry.Instance.TryGetBitmapId($"Marker_{pinName}_{colorInHex}", out int bitmapId))
            return bitmapId;

        if (!_defaultPins.TryGetValue(MarkerType, out var svg))
            return -1;

        svg = svg.Replace("red", $"#{colorInHex}");

        return BitmapRegistry.Instance.Register(svg, $"Marker_{pinName}_{colorInHex}");
    }

    private void AssertDefaultPins()
    {
        if (_defaultPins.Count == 0)
        {
            foreach (MarkerType type in Enum.GetValues(typeof(MarkerType)))
            {
                if (type == MarkerType.Icon || type == MarkerType.Svg)
                    continue;

                // Load SVGs for Pins
                using (var s = new StreamReader(EmbeddedResourceLoader.Load($"Resources.Images.{type.ToString()}.svg", typeof(Marker))))
                {
                    var svg = s.ReadToEnd();
                    _defaultPins.Add(type, svg);
                }
            }
        }
    }
}
