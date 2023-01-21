namespace Mapsui.Utilities;

public class AnimationEventArgs
{
    public AnimationEventArgs(double value, ChangeType changeType)
    {
        Value = value;
        ChangeType = changeType;
    }

    public double Value { get; }

    public ChangeType ChangeType { get; }
}
