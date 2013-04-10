using System;
using Mapsui.UI.MonoGame_W8;

namespace Mapsui.Samples.MonoGame_W8
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var factory = new MonoGame.Framework.GameFrameworkViewSource<MapControl>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
}
