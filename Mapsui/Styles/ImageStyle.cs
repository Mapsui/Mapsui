namespace Mapsui.Styles;

public class ImageStyle : BasePointStyle, IHasImage
{
    /// <summary>
    /// Path to the the image to display during rendering. This can be url, file path or embedded resource.
    /// </summary>
    public Image? Image { get; set; }
}
