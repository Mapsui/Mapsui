using System;

#if __MAUI__
namespace Mapsui.UI.Maui;
#elif __UWP__
namespace Mapsui.UI.Uwp;
#elif __ANDROID__ && !HAS_UNO_WINUI
namespace Mapsui.UI.Android;
#elif __IOS__ && !HAS_UNO_WINUI && !__FORMS__
namespace Mapsui.UI.iOS;
#elif __WINUI__
namespace Mapsui.UI.WinUI;
#elif __FORMS__
namespace Mapsui.UI.Forms;
#elif __AVALONIA__
namespace Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto;
#elif __BLAZOR__
namespace Mapsui.UI.Blazor;
#elif __WPF__
namespace Mapsui.UI.Wpf;
#else
namespace Mapsui.UI;
#endif

/// <summary>
/// Interface for objects that are clickable
/// </summary>
internal interface IClickable
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="T:Mapsui.UI.Objects.IClickable"/> is clickable.
    /// </summary>
    /// <value><c>true</c> if is clickable; otherwise, <c>false</c>.</value>
    bool IsClickable { get; }

    /// <summary>
    /// Handle click event
    /// </summary>
    /// <param name="e">Event args for drawable clicked</param>
    void HandleClicked(DrawableClickedEventArgs e);

    /// <summary>
    /// Get information, when this object is clicked
    /// </summary>
    event EventHandler<DrawableClickedEventArgs> Clicked;
}
