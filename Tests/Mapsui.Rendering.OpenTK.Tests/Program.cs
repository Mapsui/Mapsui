using System;

namespace Mapsui.Rendering.OpenTK.Tests
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var mw = new MainWindow())
            {
                mw.Run(200);
            }
        }
    }
}
