// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using Mapsui.Logging;
using System.Reflection;

namespace Mapsui.Utilities;

/// <summary>
///     Version information helper class
/// </summary>
public static class MapsuiVersion
{
    /// <summary>
    ///     Returns the current build version of Mapsui
    /// </summary>
    /// <returns></returns>
    public static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (informationalVersion == null)
        {
            Logger.Log(LogLevel.Information, "InformationVersion was not found in assembly");
            return string.Empty;
        }

        // Remove the build metadata (everything after the '+')
        return informationalVersion.Split('+')[0];
    }
}
