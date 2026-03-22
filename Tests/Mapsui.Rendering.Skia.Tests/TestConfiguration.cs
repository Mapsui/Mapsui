// TestConfiguration runs once before any test in this assembly (NUnit SetUpFixture).
// It reads config.json for default settings and config.local.json for per-machine overrides.
//
// config.json         — committed, default settings (experimentalRenderer: false).
// config.local.json   — git-ignored, overrides config.json when present.
//
// To switch to the experimental renderer locally, either:
//   - Edit config.json (affects everyone) and set experimentalRenderer to true, or
//   - Create config.local.json next to the .csproj with { "experimentalRenderer": true }
//     and rebuild — the file is copied to the output directory automatically.

using Mapsui.Experimental.Rendering.Skia;
using Mapsui.Rendering.Skia.Tests;
using NUnit.Framework;
using System.Text.Json;

[SetUpFixture]
public class TestConfiguration
{
    [OneTimeSetUp]
    public void Setup()
    {
        var dir = TestContext.CurrentContext.TestDirectory;
        // Read default config first, then let config.local.json override it.
        var cfg = ReadConfig(System.IO.Path.Combine(dir, "config.json"));
        var local = ReadConfig(System.IO.Path.Combine(dir, "config.local.json"));
        if (local is not null) cfg = local;

        if (cfg?.ExperimentalRenderer == true)
            RegressionMapControl.CreateRenderer = () => new MapRenderer();
    }

    private static LocalConfig? ReadConfig(string path)
    {
        if (!System.IO.File.Exists(path)) return null;
        return JsonSerializer.Deserialize<LocalConfig>(System.IO.File.ReadAllText(path));
    }

    private sealed class LocalConfig
    {
        public bool ExperimentalRenderer { get; set; }
    }
}
