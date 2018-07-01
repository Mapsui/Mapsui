
using Mapsui.Geometries;

namespace Mapsui.Widgets
{
    public class Hyperlink : TextBox
    {
        public string Url { get; set; }

        public override void HandleWidgetTouched(INavigator navigator, Point position)
        {
        }
    }
}
