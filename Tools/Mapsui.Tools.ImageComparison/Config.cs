namespace Mapsui.Tools.ImageComparison;

static class Config
{
    public const string RootPath = @"C:\code\github\Mapsui\Tests\Mapsui.Rendering.Skia.Tests";
    public const string OriginalRelPath = "Resources/Images/OriginalRegression";
    // NOTE: 'net9.0' must match the test project TFM; update if .NET version changes
    public const string GenRelPath = "bin/Debug/net9.0/Resources/Images/GeneratedRegression";
}
