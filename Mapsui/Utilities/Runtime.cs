using System.Runtime.InteropServices;

namespace Mapsui.Utilities;

public class Runtime
{
    private static bool? isWasm;

    public static bool IsWasm
    {
        get
        {
            return isWasm ??= RuntimeInformation.FrameworkDescription.Contains("WebAssembly");
        }
    }
}
