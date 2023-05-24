using System;

namespace Mapsui.Layers.AnimationLayers;

internal class AnimatedFeature
{
    // Todo: This is not a feature but holds state related to the animation.
    // Either turn this into a feature, and perhaps just update it, instead of creating a new.
    // Or rename this to AnimatedFeatureState or something like that.
    public AnimatedFeature(PointFeature feature, MPoint? previousPosition)
    {
        // Todo: Use the current animated position instead of the previousPosition
        // so that the new animation starts from the current position

        Destination = new MPoint(feature.Point);
        Feature = feature;
        if (previousPosition is not null)
        {
            Feature.Point.X = previousPosition.X;
            Feature.Point.Y = previousPosition.Y;
            Origin = new MPoint(previousPosition);
        }
        StartTimeInTicks = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public PointFeature Feature { get; }
    public MPoint? Destination { get; set; }
    public MPoint? Origin { get; }
    public long StartTimeInTicks { get; }
}
