using Mapsui.Rendering;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mapsui.Samples.Common;

/// <summary>
/// Reads renderer configuration from <c>config.json</c> / <c>config.local.json</c> at the
/// repository root and activates the appropriate renderer globally via
/// <see cref="DefaultRendererFactory.Create"/>.
///
/// Call <see cref="ApplyRendererConfig"/> once, early in application startup (before any
/// <c>MapControl</c> is created), typically from a static constructor.
///
/// config.json        — committed, default settings (experimentalRenderer: false).
/// config.local.json  — git-ignored, overrides config.json when present.
///
/// The repository root is located by walking up the directory tree from the executing
/// assembly until a directory containing <c>Mapsui.slnx</c> is found.  On devices
/// (Android, iOS) the file is never found and the non-experimental renderer is used.
/// </summary>
public static class SampleConfiguration
{
    public static bool IsExperimentalRenderer { get; } = ReadIsExperimental();

    /// <summary>
    /// Sets <see cref="DefaultRendererFactory.Create"/> according to the repository
    /// config file.  Must be called before the first <c>MapControl</c> is created so
    /// that the factory is in place when <c>RenderController</c> initialises its renderer.
    /// </summary>
    public static void ApplyRendererConfig()
    {
        if (IsExperimentalRenderer)
            DefaultRendererFactory.Create = () => new Mapsui.Experimental.Rendering.Skia.MapRenderer();
        else
            DefaultRendererFactory.Create = () => new Mapsui.Rendering.Skia.MapRenderer();
    }

    private static bool ReadIsExperimental()
    {
        var root = FindRepoRoot();
        if (root == null) return false;

        var cfg = ReadConfig(Path.Combine(root, "config.local.json"))
                  ?? ReadConfig(Path.Combine(root, "config.json"));
        return cfg?.ExperimentalRenderer == true;
    }

    /// <summary>
    /// Walks up the directory tree from <see cref="AppContext.BaseDirectory"/> looking
    /// for the repository root, identified by the presence of <c>Mapsui.slnx</c>.
    /// </summary>
    private static string? FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 10; i++)
        {
            if (dir == null) break;
            if (File.Exists(Path.Combine(dir, "Mapsui.slnx")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    private static SampleRendererConfig? ReadConfig(string path)
    {
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize(File.ReadAllText(path), SampleRendererConfigContext.Default.SampleRendererConfig);
    }
}

internal sealed class SampleRendererConfig
{
    public bool ExperimentalRenderer { get; set; }
}

[JsonSerializable(typeof(SampleRendererConfig))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
internal sealed partial class SampleRendererConfigContext : JsonSerializerContext
{
}
