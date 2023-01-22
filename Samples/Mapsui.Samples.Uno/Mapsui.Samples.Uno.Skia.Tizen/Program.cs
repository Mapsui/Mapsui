using Tizen.Applications;
using Uno.UI.Runtime.Skia;
using Mapsui.Samples.Uwp;

namespace Mapsui.Samples.Uno.Skia.Tizen;

class Program
{
    static void Main(string[] args)
    {
        var host = new TizenHost(() => new Mapsui.Samples.Uwp.App(), args);
        host.Run();
    }
}
