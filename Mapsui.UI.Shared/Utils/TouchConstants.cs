using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.UI.Utils;

internal class TouchConstants
{
    // See http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/4.0.4_r2.1/android/view/ViewConfiguration.java#ViewConfiguration.0PRESSED_STATE_DURATION for values
    public const int ShortTap = 125;
    public const int ShortClick = 250;
    public const int DelayTap = 200;
    public const int longTap = 500;

    /// <summary>
    /// If a finger touches down and up it counts as a tap if the distance between the down and up location is smaller
    /// then the touch slob.
    /// The slob is initialized at 8. How did we get to 8? Well you could read the discussion here: https://github.com/Mapsui/Mapsui/issues/602
    /// We basically copied it from the Java source code: https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/view/ViewConfiguration.java#162
    /// </summary>
    public const int TouchSlop = 8;
}
