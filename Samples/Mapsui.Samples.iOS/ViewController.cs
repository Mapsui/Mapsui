using Mapsui.UI.iOS;
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
        View = mapControl;
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
