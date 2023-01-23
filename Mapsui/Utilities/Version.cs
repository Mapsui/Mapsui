// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System.Reflection;

namespace Mapsui.Utilities;

/// <summary>
///     Version information helper class
/// </summary>
public static class Version
{
    /// <summary>
    ///     Returns the current build version of Mapsui
    /// </summary>
    /// <returns></returns>
    public static System.Version? GetCurrentVersion()
    {
        var assembly = typeof(Version).GetTypeInfo().Assembly;
        // In some PCL profiles the above line is: var assembly = typeof(MyType).Assembly;
        if (assembly.FullName == null)
            return null;

        var assemblyName = new AssemblyName(assembly.FullName);
        return assemblyName.Version;
    }
}
