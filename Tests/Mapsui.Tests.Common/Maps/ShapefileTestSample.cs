using System.IO;
using System.Threading.Tasks;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Utilities;

namespace Mapsui.Tests.Common.Maps;

public class ShapefileTestSample : ISample
{
    static ShapefileTestSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("test.shp");
    }

    public string Name => "Shapefile Zoom";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        
        var shapeFilePath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "test.shp");
        ShapeFile shpSource = new ShapeFile( shapeFilePath );
        /* Apply basic styles */
        Mapsui.Layers.Layer layer = new Mapsui.Layers.Layer() {
            DataSource = shpSource,
            Style = new Mapsui.Styles.VectorStyle {
                Fill = new Mapsui.Styles.Brush( Mapsui.Styles.Color.FromArgb( 128, 0 , 255 , 0 ) ) ,
                Line = new Mapsui.Styles.Pen( Mapsui.Styles.Color.FromString("#0969da") , 4 ){PenStrokeCap = Mapsui.Styles.PenStrokeCap.Round, StrokeJoin = Mapsui.Styles.StrokeJoin.Round}
            }
        };
        /* Add the new layer */
        map.Layers.Add( layer );

        return map;
    }
}
