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
}
