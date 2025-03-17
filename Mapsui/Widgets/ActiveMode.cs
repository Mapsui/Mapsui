namespace Mapsui.Widgets;

public enum ActiveMode
{
    /// <summary>
    /// Show logging in the map. Note, this only has effect if LoggingWidget.Enabled == true.
    /// </summary>
    Yes,
    /// <summary>
    /// Show logging in the map only if the debugger is attached. Note, this is independent of a debug build.
    /// You can attach a debugger to a release build and it will show logging, or run a debug build without
    /// a debugger attached and it won't show logging.
    /// </summary>
    OnlyInDebugMode,
    /// <summary>
    /// Never show logging in the map.
    /// </summary>
    No,
}
