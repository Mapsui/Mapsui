using Mapsui.Widgets;
using Topten.RichTextKit;

namespace Mapsui.Rendering.Skia.Extensions;

public static class AlignmentExtensions
{
    public static TextAlignment ToRichTextKit(this Alignment textAlignment)
    {
        switch (textAlignment)
        {
            case Alignment.Left:
                return TextAlignment.Left;
            case Alignment.Center:
                return TextAlignment.Center;
            case Alignment.Right:
                return TextAlignment.Right;
            default:
                return TextAlignment.Left;
        }
    }
}
