#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

public enum PinType
{
    /// <summary>
    /// Pin with image, which could change color
    /// </summary>
    Pin,

    /// <summary>
    /// Pin as icon image
    /// </summary>
    Icon,

    /// <summary>
    /// Pin as Svg image
    /// </summary>
    Svg
}
