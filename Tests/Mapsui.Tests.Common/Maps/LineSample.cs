using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using NetTopologySuite.IO;

namespace Mapsui.Tests.Common.Maps;

public class LineSample : IMapControlSample
{
    public string Name => "Line";
    public string Category => "Tests";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
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
            Style = null,
            Features = CreateLineFeatures(),
            Name = "Line"
        };
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning")]
    private static IEnumerable<IFeature> CreateLineFeatures()
    {
        var features = new List<IFeature>();
        var wktReader = new WKTReader();
        var lineStringAsTest = "LINESTRING (-642227.831499647 5123550.9224083, -1241640.47423265 5037920.54487501, -1584161.9843658 4923746.70816396, -1869596.57614342 4923746.70816396, -2012313.87203223 4838116.33063068, -2326291.92298761 4809572.87145292, -2583183.05558747 4723942.49391963, -3211139.15749824 4695399.03474187, -3468030.2900981 4552681.73885306, -3610747.58598691 4524138.27967529, -3753464.88187572 4409964.44296425, -3810551.80023124 4409964.44296425, -4124529.85118663 4095986.39200886, -4181616.76954215 4095986.39200886, -4267247.14707544 4010356.01447558, -4467051.36131977 3924725.63694229, -4524138.2796753 3839095.259409, -4923746.70816397 3553660.66763138, -5066464.00405278 3525117.20845362, -5152094.38158606 3439486.83092033, -5237724.75911935 3268226.07585376, -5466072.43254144 3125508.77996495, -5523159.35089697 2982791.48407614, -5780050.48349683 2668813.43312076, -5979854.69774116 2354835.38216537, -6008398.15691892 2240661.54545433, -6065485.07527445 2183574.6270988, -6122571.99362997 1898140.03532118, -6236745.83034102 1755422.73943237, -6265289.28951878 1612705.44354356, -6322376.20787431 842032.045743983, -6322376.20787431 -642227.831499647, -6265289.28951878 -1041836.25998832, -6151115.45280774 -1527075.06601027, -5865680.86103011 -2240661.54545433, -5751507.02431907 -2383378.84134314, -5694420.10596354 -2526096.13723195, -5551702.81007473 -2668813.43312076, -5523159.35089697 -2754443.81065404, -5009377.08569725 -3239682.616676, -4781029.41227515 -3353856.45338705, -4609768.65720858 -3382399.91256481, -4295790.6062532 -3553660.66763138, -3525117.20845362 -3810551.80023124, -3325312.99420929 -3953269.09612005, -2554639.59640971 -4153073.31036439, -2069400.79038775 -4324334.06543096, -984749.341632795 -4524138.27967529, -670771.29067741 -4609768.65720858, -185532.484655455 -4809572.87145291, 271162.862188738 -4895203.2489862, 870575.504921742 -5180637.84076382, 1641248.90272132 -5437528.97336368, 1869596.57614342 -5551702.81007473, 2297748.46380985 -5637333.18760802, 2725900.35147628 -5637333.18760802, 3353856.45338705 -5694420.10596354, 4980833.62651949 -5722963.5651413, 4980833.62651949 -5694420.10596354, 5180637.84076382 -5665876.64678578, 5237724.75911935 -5608789.72843025, 5665876.64678578 -5523159.35089697, 5922767.77938564 -5323355.13665263, 6293832.74869654 -5209181.29994159, 6465093.50376312 -5095007.46323054, 6579267.34047417 -5095007.46323054, 6779071.5547185 -5009377.08569725, 6893245.39142955 -4895203.2489862, 7321397.27909598 -4866659.78980844, 7492658.03416255 -4809572.87145291, 7749549.16676241 -4809572.87145291, 8377505.26867318 -4638312.11638634, 8891287.53387289 -4609768.65720858, 9262352.5031838 -4723942.49391963, 9604874.01331695 -5037920.54487501, 9690504.39085023 -5066464.00405278, 9861765.14591681 -5266268.21829711, 10204286.65605 -5494615.89171921, 10946416.5946718 -5751507.02431907, 11888350.7475379 -5922767.77938564, 12830284.9004041 -5979854.69774116, 14971044.3387362 -5979854.69774116, 15427739.6855804 -5894224.32020788, 15741717.7365358 -5751507.02431907, 15770261.1957136 -5694420.10596354, 15827348.1140691 -5694420.10596354, 16226956.5425578 -5180637.84076382, 16369673.8384466 -4524138.27967529, 16483847.6751576 -4295790.6062532, 16512391.1343354 -4295790.6062532, 16540934.5935131 -4153073.31036439, 16598021.5118687 -4067442.9328311, 16626564.9710464 -3867638.71858677, 16769282.2669352 -3610747.58598691, 16797825.726113 -2811530.72900957, 16854912.6444685 -2611726.51476523, 16854912.6444685 -2383378.84134314, 16911999.5628241 -2240661.54545433, 16940543.0220018 -1726879.28025461, 17026173.3995351 -1555618.52518803, 17140347.2362461 -1469988.14765475, 17197434.1546017 -1327270.85176594, 17654129.5014459 -727858.209032934, 17739759.8789792 -528053.994788598, 17739759.8789792 -356793.239722027)";
        var feature = new GeometryFeature(wktReader.Read(lineStringAsTest));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Violet, 5) });
        features.Add(feature);
        // Add some dashed lines
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 12000000, -1000000 12000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.Dash } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (00000 12000000, 6000000 12000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Green) { PenStyle = PenStyle.ShortDash } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (7000000 12000000, 13000000 12000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.DashDot } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (14000000 12000000, 20000000 12000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.ShortDashDot } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 11000000, -1000000 11000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Green) { PenStyle = PenStyle.DashDotDot } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (000000 11000000, 6000000 11000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.ShortDashDotDot } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (7000000 11000000, 13000000 11000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.Dot } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (14000000 11000000, 20000000 11000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Green) { PenStyle = PenStyle.ShortDot } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 10000000, -1000000 10000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.LongDash } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 9750000, -1000000 9750000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.LongDash, DashOffset = 2 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 9500000, -1000000 9500000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.LongDash, DashOffset = 4 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 9250000, -1000000 9250000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.LongDash, DashOffset = 6 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 9000000, -1000000 9000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.LongDash, DashOffset = 8 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (00000 10000000, 6000000 10000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.UserDefined, DashArray = new float[] { 6, 4, 12, 4, 2, 4, 12, 4 } } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (00000 9750000, 6000000 9750000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.UserDefined, DashArray = new float[] { 6, 4, 12, 4, 2, 4, 12, 4 }, DashOffset = 5 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (00000 9500000, 6000000 9500000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.UserDefined, DashArray = new float[] { 6, 4, 12, 4, 2, 4, 12, 4 }, DashOffset = 10 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (00000 9250000, 6000000 9250000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.UserDefined, DashArray = new float[] { 6, 4, 12, 4, 2, 4, 12, 4 }, DashOffset = 15 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (00000 9000000, 6000000 9000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.UserDefined, DashArray = new float[] { 6, 4, 12, 4, 2, 4, 12, 4 }, DashOffset = 20 } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (7000000 10000000, 13000000 10000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Green) { PenStyle = PenStyle.LongDashDot } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (14000000 10000000, 20000000 10000000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue) { PenStyle = PenStyle.Solid } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (-7000000 7500000, -3725000 7500000, -3725000 6500000, -1000000 6500000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Gray, 15) { PenStyle = PenStyle.Solid, PenStrokeCap = PenStrokeCap.Butt, StrokeJoin = StrokeJoin.Bevel, StrokeMiterLimit = 1 } });
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.Solid, PenStrokeCap = PenStrokeCap.Butt } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (000000 7500000, 3000000 7500000, 3000000 6500000, 6000000 6500000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Gray, 15) { PenStyle = PenStyle.Solid, PenStrokeCap = PenStrokeCap.Round, StrokeJoin = StrokeJoin.Round, StrokeMiterLimit = 1 } });
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.Solid, PenStrokeCap = PenStrokeCap.Butt } });
        features.Add(feature);
        feature = new GeometryFeature(wktReader.Read("LINESTRING (7500000 7500000, 10000000 7500000, 10000000 6500000, 12500000 6500000)"));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Gray, 15) { PenStyle = PenStyle.Solid, PenStrokeCap = PenStrokeCap.Square, StrokeJoin = StrokeJoin.Miter } });
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.Red) { PenStyle = PenStyle.Solid, PenStrokeCap = PenStrokeCap.Butt } });
        features.Add(feature);

        return features;
    }
}
