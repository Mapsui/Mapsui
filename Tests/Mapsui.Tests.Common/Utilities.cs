namespace Mapsui.Tests.Common;

public static class Utilities
{
    // When there is no explicit call to the assembly it is not loaded
    // Even if there is a project reference. Calling this method is the workaround.
    public static void LoadAssembly() { }
}
