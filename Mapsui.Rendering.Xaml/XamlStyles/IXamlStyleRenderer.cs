using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Rendering.Xaml
{
    /// <summary>
    /// The IXamlStyleRenderer interface is not public because xaml rendering will
    /// probably be removed on some point and we do not want users to go further
    /// in that direction. 
    /// </summary>
    interface IXamlStyleRenderer : IStyleRenderer
    {
        void Render(RenderStyleEventArgs args);
    }
}
