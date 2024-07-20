using Mapsui.Samples.WindowsForms;

namespace Mapsui.Samples.WinForms;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
#pragma warning disable IDISP004
        Application.Run(new SampleWindow());
#pragma warning restore IDISP004
    }
}
