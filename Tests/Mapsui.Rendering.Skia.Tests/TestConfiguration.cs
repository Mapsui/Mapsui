// TestConfiguration runs once before any test in this assembly (NUnit SetUpFixture).
// It reads config files in priority order:
//   1. config.local.json in the test binary directory   — per-machine override, git-ignored
//   2. config.json in the test binary directory         — project-level default (experimentalRenderer: false)
//   3. config.local.json at the repository root         — per-machine override, git-ignored
//   4. config.json at the repository root               — repo-wide default
//
// The repository root is located by walking up from the test binary directory until
// a directory containing Mapsui.slnx is found.
//
// To switch to the experimental renderer locally:
//   - Create config.local.json at the repository root with { "experimentalRenderer": true }, or
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

        // Binary-dir configs take priority (allow per-project overrides).
        var cfg = ReadConfig(System.IO.Path.Combine(dir, "config.local.json"))
                  ?? ReadConfig(System.IO.Path.Combine(dir, "config.json"));
        if (cfg != null) return cfg.ExperimentalRenderer;

        // Fall back to the repository-root config.
        var root = FindRepoRoot(dir);
        if (root != null)
            cfg = ReadConfig(System.IO.Path.Combine(root, "config.local.json"))
                  ?? ReadConfig(System.IO.Path.Combine(root, "config.json"));
        return cfg?.ExperimentalRenderer == true;
    }

    private static string? FindRepoRoot(string startDir)
    {
        var dir = startDir;
        for (var i = 0; i < 10; i++)
        {
            if (dir == null) break;
            if (System.IO.File.Exists(System.IO.Path.Combine(dir, "Mapsui.slnx")))
                return dir;
            dir = System.IO.Path.GetDirectoryName(dir);
        }
        return null;
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
