using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Utilities;
using System.IO;

namespace Mapsui.Features;

internal class Marker : PointFeature
{
    private static string? _defaultPin;

    private readonly SymbolStyle _style = new SymbolStyle();

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
            if (_markerType == value)
                return;
            _markerType = value;
        }
    }

    private Color _color = Color.Red;
    
    /// <summary>
    /// Color for pin
    /// </summary>
    public Color Color
    { 
        get => _color;
        set
        {
            if (_color == value)
                return;
            _color = value;
        }
    }

    /// <summary>
    /// Initialize marker
    /// </summary>
    /// <param name="type"></param>
    private void InitMarker(MarkerType type)
    {
        AssertDefaultPin();

        // Remove all existing styles
        Styles.Clear();
        
        // Set default values for style
        _style.Enabled = true;
        _style.SymbolType = SymbolType.Image;
        _style.SymbolOffset = new Offset(0.0, 0.5, true);
        _style.BitmapId = GetPinId();

        // Add style to Styles for this feature
        Styles.Add(_style);
    }

    private int GetPinId()
    {
        return GetPinWithColor(Color);
    }

    private int GetIconId() 
    {
        return 0;
    }

    private int GetSvgId() 
    {
        return 0;
    }

    private int GetPinWithColor(Color color)
    {
        var colorInHex = $"{Color.R:X2}{Color.G:X2}{Color.B:X2}";
        var pinWithColor = _defaultPin?.Replace("#000000", $"#{colorInHex}") ?? "Default";

        return BitmapRegistry.Instance.Register(pinWithColor, $"Pin.{colorInHex}");

    }

    private void AssertDefaultPin()
    {
        if (_defaultPin == null)
        {
            // Load SVG for Pin
            var assembly = typeof(Marker).Assembly;

            using (var s = new StreamReader(EmbeddedResourceLoader.Load(@"Resources.Images.Pin.svg", typeof(Marker))))
            {
                _defaultPin = s.ReadToEnd();
                BitmapRegistry.Instance.Register(_defaultPin, "Pin.Default");
            }
        }
    }
}
