using Mapsui.Nts;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public interface IPin
{
    GeometryFeature? Feature { get; }
    bool IsVisible { get; set; }
    ICallout Callout { get; }
    bool RotateWithMap { get; set; }

    /// <summary>
    /// Type of pin. There are some predefined pins.
    /// </summary>
    PinType Type { get; set; }

    /// <summary>
    /// Position of pin, place where anchor is
    /// </summary>
    Position Position { get; set; }

    /// <summary>
    /// Scaling of pin
    /// </summary>
    float Scale { get; set; }

    /// <summary>
    /// Label of pin
    /// </summary>
    string Label { get; set; }

    /// <summary>
    /// Adress (like street) of pin
    /// </summary>
    string Address { get; set; }

    /// <summary>
    /// Byte[] holding the bitmap informations
    /// </summary>
    byte[] Icon { get; set; }

    /// <summary>
    /// String holding the Svg image informations
    /// </summary>
    string Svg { get; set; }

    /// <summary>
    /// Rotation in degrees around the anchor point
    /// </summary>
    float Rotation { get; set; }

    /// <summary>
    /// MinVisible for pin in resolution of Mapsui (smaller values are higher zoom levels)
    /// </summary>
    double MinVisible { get; set; }

    /// <summary>
    /// MaxVisible for pin in resolution of Mapsui (smaller values are higher zoom levels)
    /// </summary>
    double MaxVisible { get; set; }

    /// <summary>
    /// Width of the bitmap after scaling, if there is one, if not, than -1
    /// </summary>
    double Width { get; }

    /// <summary>
    /// Height of the bitmap after scaling, if there is one, if not, than -1
    /// </summary>
    double Height { get; }

    /// <summary>
    /// Transparency of pin
    /// </summary>
    float Transparency { get; set; }

    /// <summary>
    /// Tag holding free data
    /// </summary>
    object? Tag { get; set; }

    void HideCallout();
    void ShowCallout();
    bool IsCalloutVisible();
}
