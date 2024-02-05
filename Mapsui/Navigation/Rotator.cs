namespace Mapsui;

public class Rotator
{
    /// <summary>
    /// After how many degrees start rotation to take place
    /// </summary>
    public double UnSnapRotationDegrees { get; set; } = 30;

    /// <summary>
    /// With how many degrees from 0 should map snap to 0 degrees
    /// </summary>
    public double ReSnapRotationDegrees { get; set; } = 5;

    public double VirtualRotation { get; set; }
}
