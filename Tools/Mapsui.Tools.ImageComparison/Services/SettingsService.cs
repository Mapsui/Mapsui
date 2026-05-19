using System;
using System.IO;
using System.Text.Json;

namespace Mapsui.Tools.ImageComparison.Services;

class SettingsService
{
    static readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Mapsui", "ImageComparison", "settings.json");

    record SettingsData(string RootPath);

    public string LoadRootPath()
    {
        try
        {
            if (!File.Exists(_path)) return Config.RootPath;
            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_path));
            return data?.RootPath ?? Config.RootPath;
        }
        catch
        {
            return Config.RootPath;
        }
    }

    public void SaveRootPath(string rootPath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(new SettingsData(rootPath)));
        }
        catch { /* best effort */ }
    }
}
