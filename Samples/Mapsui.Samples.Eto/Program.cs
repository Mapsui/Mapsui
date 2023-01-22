#pragma warning disable IDISP004 // Don't ignore IDisposable

namespace Mapsui.Samples.Eto;

using System;
using global::Eto;
using global::Eto.Forms;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        new Application(Platform.Detect).Run(new MainForm());
    }
}
