using System.IO;
using System.Linq;
using NLog;
using VexTile.Common.Enums;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public static class VectorStyleReader
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static string[] names;

    public static string GetStyle(VectorStyleKind styleKind)
    {
        var text = styleKind.ToString().ToLower();
        // Read from the AliFlux assembly which contains the embedded style resources
        var aliFluxAssembly = typeof(VexTile.Renderer.Mvt.AliFlux.IVectorStyle).Assembly;
        var name = aliFluxAssembly.GetName().Name;
        var name2 = name + ".Styles." + text + "-style.json";
        using var stream = aliFluxAssembly.GetManifestResourceStream(name2);
        if (stream == null)
            throw new VectorStyleException($"Could not find '{name}.Styles.{text}-style.json'");
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }

    public static bool TryGetFont(string name, out Stream stream)
    {
        try
        {
            name = name.Replace(' ', '-');
            // Read from the AliFlux assembly which contains the embedded font resources
            var aliFluxAssembly = typeof(VexTile.Renderer.Mvt.AliFlux.IVectorStyle).Assembly;
            var name2 = aliFluxAssembly.GetName().Name;
            var resourceName = name2 + ".Styles.fonts." + name;
            if (names == null)
                names = aliFluxAssembly.GetManifestResourceNames();
            var text = names?.FirstOrDefault(x => x.StartsWith(resourceName));
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var stream2 = aliFluxAssembly.GetManifestResourceStream(text);
                if (stream2 != null)
                {
                    stream = new MemoryStream();
                    stream2.CopyTo(stream);
                    stream.Seek(0L, SeekOrigin.Begin);
                    return true;
                }
            }
        }
        catch (System.Exception value)
        {
            Log.Error(value);
        }
        stream = null;
        return false;
    }
}
