using System.Diagnostics;
using System.Text;
using Mapsui.UI.iOS;
using Mapsui.UI;
using Mapsui.Samples.Common.Maps.Demo;

namespace Mapsui.Samples.iOS;

public partial class ViewController : UIViewController
{
    public ViewController() : base()
    {
        MapControl.UseGPU = true;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        var mapControl = CreateMap(View!.Bounds);
        mapControl.Info += MapOnInfo;
        View = mapControl;
    }

    private void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        if (e.MapInfo?.Feature == null) return;
        Debug.WriteLine(ToString(e.MapInfo.Feature));
    }

    private string ToString(IFeature feature)
    {
        var result = new StringBuilder();
        foreach (var field in feature.Fields)
        {
            result.Append($"{field}={feature[field]}, ");
        }

        return result.ToString();
    }

    private static MapControl CreateMap(CGRect bounds)
    {
        return new MapControl(bounds)
        {
            Map = InfoLayersSample.CreateMap(),
            UnSnapRotationDegrees = 30,
            ReSnapRotationDegrees = 5
        };
    }
}
