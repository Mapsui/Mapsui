using Mapsui.UI.Forms;
using System;

namespace Mapsui.UI.Objects
{
    interface IClickable
    {
        // Is this object clickable?
        bool IsClickable { get; }

        // Handle click event
        void HandleClicked(DrawableClickedEventArgs e);

        // Get information, when this object is clicked
        event EventHandler<DrawableClickedEventArgs> Clicked;
    }
}
