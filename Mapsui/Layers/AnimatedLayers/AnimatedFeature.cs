using System.Text;

namespace Mapsui.Layers.AnimationLayers;

internal class AnimatedFeature
{
    public AnimatedFeature(PointFeature feature, MPoint? previousPoint)
    {
        Feature = feature;
        CurrentPoint = new MPoint(feature.Point);
        if (previousPoint != null)
            PreviousPoint = new MPoint(previousPoint);
    }
    public PointFeature Feature { get; }
    public MPoint? CurrentPoint { get; set; }
    public MPoint? PreviousPoint { get; }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("Trackee: " + Feature?["Trackee"]);
        stringBuilder.Append(", ID: " + Feature?["ID"]);
        stringBuilder.Append(", speed: " + Feature?["Speed"]);
        stringBuilder.Append(", Bps: " + Feature?["Bps"]);
        stringBuilder.Append(", DateGps: " + Feature?["DateGps"]);
        stringBuilder.Append(", DateReceived: " + Feature?["DateReceived"]);
        stringBuilder.Append(", Longitude: " + Feature?["Longitude"]);
        stringBuilder.Append(", Latitude: " + Feature?["Latitude"]);
        stringBuilder.Append(", X: " + CurrentPoint?.X);
        stringBuilder.Append(", Y: " + CurrentPoint?.Y);
        stringBuilder.Append(", Previous X: " + PreviousPoint?.X);
        stringBuilder.Append(", Previous Y: " + PreviousPoint?.Y);
        return stringBuilder.ToString();
    }
}
