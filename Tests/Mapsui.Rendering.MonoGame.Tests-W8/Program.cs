using MonoGame.Framework;

namespace Mapsui.Rendering.MonoGame.Tests_W8
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
            var path = Windows.Storage.ApplicationData.Current.RoamingFolder.Path;
            var factory = new GameFrameworkViewSource<MonoGameTester>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
}
