using System.Reflection;

namespace Mapsui.Utilities
{
    /// <summary>
    ///     Version information helper class
    /// </summary>
    public static class Version
    {
        /// <summary>
        ///     Returns the current build version of Mapsui
        /// </summary>
        /// <returns></returns>
        public static System.Version GetCurrentVersion()
        {
            var assembly = typeof(Version).GetTypeInfo().Assembly;
            // In some PCL profiles the above line is: var assembly = typeof(MyType).Assembly;
            var assemblyName = new AssemblyName(assembly.FullName);
            return assemblyName.Version;
        }
    }
}