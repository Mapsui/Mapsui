using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using NetTopologySuite.IO;

namespace Mapsui.Tests.Common.Maps;

public class PolygonTestSample : IMapControlSample
{
    private static int _bitmapId;

    public string Name => "Polygon";
    public string Category => "Tests";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        _bitmapId = typeof(PolygonTestSample).LoadBitmapId("Resources.Images.avion_silhouette.png");

        var layer = CreateLayer();

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateToFullEnvelope(ScaleMethod.Fit)
        };

        map.Layers.Add(layer);

        return map;
    }

    private static MemoryLayer CreateLayer()
    {
        return new MemoryLayer
        {
            Features = CreatePolygonProvider(),
            Name = "Polygon"
        };
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning")]
    public static IEnumerable<IFeature> CreatePolygonProvider()
    {
        var wktReader = new WKTReader();
        var features = new List<IFeature>();

        var feature = new GeometryFeature
        {
            Geometry = wktReader.Read(
                "POLYGON ((-1955226.9536767 4267247.14707544, -2183574.6270988 4267247.14707544, -2611726.51476523 4095986.39200886, -2868617.64736509 4067442.9328311, -3268226.07585376 3896182.17776453, -3667834.50434243 3839095.259409, -4067442.9328311 3639291.04516467, -4495594.82049753 3525117.20845362, -4809572.87145292 3068421.86160943, -5066464.00405278 2954248.02489838, -5266268.21829711 2668813.43312076, -5780050.48349683 2240661.54545433, -6008398.15691892 1612705.44354356, -6265289.28951878 1070379.71916608, -6293832.74869655 813488.586566221, -6408006.58540759 642227.831499647, -6579267.34047417 -299706.321366502, -6779071.5547185 -984749.341632793, -6893245.39142955 -1755422.73943237, -6921788.85060731 -2982791.48407614, -6978875.76896284 -3296769.53503152, -6950332.30978507 -4866659.78980844, -6750528.09554074 -5922767.77938564, -6522180.42211864 -6522180.42211864, -6408006.58540759 -6721984.63636298, -6122571.99362997 -6950332.30978507, -6008398.15691892 -7378484.1974515, -5751507.02431907 -7692462.24840689, -5152094.38158606 -8234787.97278437, -4923746.70816397 -8348961.80949542, -3667834.50434243 -8634396.40127304, -2954248.02489838 -8720026.77880632, -899118.964099508 -8777113.69716185, -156989.025477692 -8834200.61551737, 1213097.01505489 -8834200.61551737, 2697356.89229852 -8777113.69716185, 3810551.80023124 -8634396.40127304, 4552681.73885306 -8291874.89113989, 4838116.33063068 -8120614.13607332, 5209181.29994158 -8006440.29936227, 5351898.5958304 -7920809.92182898, 5551702.81007473 -7635375.33005136, 5780050.48349683 -7521201.49334032, 5922767.77938564 -7292853.81991822, 6094028.53445221 -7150136.52402941, 6322376.20787431 -7035962.68731836, 6607810.79965193 -6779071.5547185, 6721984.63636298 -6522180.42211864, 7064506.14649612 -6065485.07527445, 7093049.60567388 -5894224.32020788, 7178679.98320717 -5694420.10596354, 7178679.98320717 -4923746.70816396, 7093049.60567388 -3981812.55529781, 6978875.76896283 -3496573.74927586, 6921788.85060731 -2925704.56572062, 6921788.85060731 -2611726.51476523, 7007419.2281406 -2497552.67805418, 7007419.2281406 -2440465.75969866, 8291874.89113989 -2155031.16792104, 10118656.2785167 -1898140.03532118, 11288938.1048049 -1555618.52518803, 12116698.42096 -870575.504921744, 12230872.2576711 -613684.372321885, 12373589.5535599 -442423.617255311, 12516306.8494487 -99902.1071221679, 12544850.3086264 642227.831499647, 12544850.3086264 4838116.33063068, 12573393.7678042 5009377.08569725, 12687567.6045153 5380442.05500816, 12915915.2779374 5665876.64678578, 13058632.5738262 5780050.48349683, 13429697.5431371 6293832.74869655, 13515327.9206704 6664897.71800745, 13515327.9206704 7035962.68731836, 13258436.7880705 7464114.57498479, 13030089.1146484 7492658.03416255, 12744654.5228708 7635375.33005136, 12544850.3086264 7692462.24840689, 11060590.4313828 7721005.70758465, 10889329.6763162 7635375.33005136, 10689525.4620719 7492658.03416255, 10575351.6253609 7292853.81991822, 10489721.2478276 7007419.2281406, 10375547.4111165 6864701.93225179, 10289917.0335832 6550723.8812964, 10204286.65605 6408006.58540759, 9947395.5234501 5437528.97336368, 9661960.93167247 4695399.03474187, 9604874.01331695 4381420.98378648, 9148178.66647276 4038899.47365334, 8948374.45222842 3953269.09612005, 8605852.94209528 3724921.42269796, 8206244.5136066 3667834.50434243, 7835179.5442957 3553660.66763138, 7606831.8708736 3553660.66763138, 7407027.65662927 3439486.83092033, 7150136.52402941 3382399.91256481, 6522180.42211864 3325312.99420929, 5951311.2385634 3154052.23914271, 4010356.01447558 3154052.23914271, 3239682.616676 3325312.99420929, 2811530.72900957 3353856.45338705, 2697356.89229852 3410943.37174257, 2212118.08627656 3496573.74927586, 2012313.87203223 3582204.12680915, 1469988.14765475 3639291.04516467, 1327270.85176594 3724921.42269796, 956205.882455029 3753464.88187572, 585140.913144123 3867638.71858677, -528053.994788598 3924725.63694229, -984749.341632795 4067442.9328311, -1070379.71916608 4124529.85118663, -1156010.09669936 4124529.85118663, -1156010.09669936 4153073.31036439, -1612705.44354356 4153073.31036439, -1869596.57614342 4210160.22871991, -1955226.9536767 4267247.14707544, -1955226.9536767 4267247.14707544))")
        };

        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(new Color(255, 0, 0, 120), FillStyle.BitmapRotated, _bitmapId),
            Outline = CreatePen(new Color(255, 255, 0), 2, PenStyle.DashDot),
            Line = null
        });

        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((2000000 10000000, 2000000 8000000, 10000000 8000000, 10000000 10000000, 2000000 10000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Blue, FillStyle.BackwardDiagonal),
            Outline = CreatePen(Color.Blue, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-8000000 10000000, 0000000 10000000, 0000000 8000000, -8000000 8000000, -8000000 10000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Red, FillStyle.Cross),
            Outline = CreatePen(Color.Red, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-18000000 10000000, -10000000 10000000, -10000000 8000000, -18000000 8000000, -18000000 10000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Gray, FillStyle.DiagonalCross),
            Outline = CreatePen(Color.Gray, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-18000000 6000000, -10000000 6000000, -10000000 4000000, -18000000 4000000, -18000000 6000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Gray, FillStyle.Dotted),
            Outline = CreatePen(Color.Gray, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-18000000 2000000, -10000000 2000000, -10000000 000000, -18000000 000000, -18000000 2000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Green, FillStyle.ForwardDiagonal),
            Outline = CreatePen(Color.Green, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-18000000 -2000000, -10000000 -2000000, -10000000 -4000000, -18000000 -4000000, -18000000 -2000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Cyan, FillStyle.Hollow),
            Outline = CreatePen(Color.Cyan, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-18000000 -6000000, -10000000 -6000000, -10000000 -8000000, -18000000 -8000000, -18000000 -6000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Indigo, FillStyle.Horizontal),
            Outline = CreatePen(Color.Indigo, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-18000000 -10000000, -10000000 -10000000, -10000000 -12000000, -18000000 -12000000, -18000000 -10000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Orange, FillStyle.Solid),
            Outline = CreatePen(Color.Orange, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        feature = new GeometryFeature
        {
            Geometry = wktReader.Read("POLYGON ((-8000000 -10000000, 0000000 -10000000, 0000000 -12000000, -8000000 -12000000, -8000000 -10000000))")
        };
        feature.Styles.Add(new VectorStyle
        {
            Enabled = true,
            Fill = CreateBrush(Color.Violet, FillStyle.Vertical),
            Outline = CreatePen(Color.Violet, 2, PenStyle.Solid),
            Line = null
        });
        features.Add(feature);

        return features;
    }

    private static Brush CreateBrush(Color color, FillStyle fillStyle, int? imageId = null)
    {
        if (imageId.HasValue && !(fillStyle == FillStyle.Bitmap || fillStyle == FillStyle.BitmapRotated))
            fillStyle = FillStyle.Bitmap;

        return new Brush { FillStyle = fillStyle, BitmapId = imageId ?? -1, Color = color };
    }

    private static Pen CreatePen(Color color, int width, PenStyle penStyle)
    {
        return new Pen(color, width) { PenStyle = penStyle };
    }

}
