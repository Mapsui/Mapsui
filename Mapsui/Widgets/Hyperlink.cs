
using Mapsui.Geometries;

namespace Mapsui.Widgets
{
    public class Hyperlink : TextBox
    {
        public string Url { get; set; }

        public override bool HandleWidgetTouched(INavigator navigator, Point position)
        {
            // Because OpenURL is called from MapControl by default
            // TODO: Shouldn't OpenURL() called from here instead of from MapControl?
            return true;
        }
    }
}
