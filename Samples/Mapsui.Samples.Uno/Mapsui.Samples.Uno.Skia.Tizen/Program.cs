using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace Mapsui.Samples.Uno.Skia.Tizen;

class Program
{
    static void Main(string[] args)
    {
        var host = new TizenHost(() => new App(), args);
        host.Run();
    }
}
