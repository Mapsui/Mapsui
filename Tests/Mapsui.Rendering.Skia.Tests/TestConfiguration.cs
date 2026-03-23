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
    // Eagerly computed from config files so it is available when NUnit evaluates
    // [TestCaseSource] at test-discovery time — before [OneTimeSetUp] runs.
    public static bool IsExperimentalRenderer { get; } = ReadIsExperimental();

    [OneTimeSetUp]
    public void Setup()
    {
        if (IsExperimentalRenderer)
            RegressionMapControl.CreateRenderer = () => new MapRenderer();
    }

    private static bool ReadIsExperimental()
    {
        var dir = System.IO.Path.GetDirectoryName(typeof(TestConfiguration).Assembly.Location)
                  ?? System.AppDomain.CurrentDomain.BaseDirectory;
        var cfg = ReadConfig(System.IO.Path.Combine(dir, "config.local.json"))
                  ?? ReadConfig(System.IO.Path.Combine(dir, "config.json"));
        return cfg?.ExperimentalRenderer == true;
    }

    private static LocalConfig? ReadConfig(string path)
    {
        if (!System.IO.File.Exists(path)) return null;
        return JsonSerializer.Deserialize<LocalConfig>(System.IO.File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private sealed class LocalConfig
    {
        public bool ExperimentalRenderer { get; set; }
    }
}
