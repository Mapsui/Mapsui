using System;

namespace Mapsui;

public class RotationSnapper
{
    /// <summary>
    /// After how many degrees start rotation to take place
    /// </summary>
    public double UnSnapRotation { get; set; } = 30;

    /// <summary>
    /// With how many degrees from 0 should map snap to 0 degrees
    /// </summary>
    public double ReSnapRotation { get; set; } = 5;

    /// <summary>
    /// The rotation in degrees of the current pinch gesture. Note that this does not say
    /// anything about the map rotation. 
    /// </summary>
    public double PinchRotation { get; set; }
     
    /// <summary>
    /// Calculates the rotation delta taking into account the snapping parameters. 
    /// </summary>
    /// <param name="rotation">The rotation of the viewport of the map. This rotation is visible in the map.</param>
    /// <returns></returns>
    public double CalculateRotationDelta(double rotation, bool rotationLock)
    {
        if (rotationLock) 
            return 0;

        if (Math.Abs(rotation) < double.Epsilon) // There is no rotation
        {
            if (RotationShortestDistance(PinchRotation, 0) >= UnSnapRotation)
                return PinchRotation; // Unsnap. The virtualRotation can be applied.
            else
                return 0; // Still snapped. No delta.
        }
        else // There is rotation
        {
            if (RotationShortestDistance(rotation + PinchRotation, 0) <= ReSnapRotation)
                return -rotation; // Resnap. Undo the actual rotation by returning the inverse as delta. 
            else
                return PinchRotation; // Still unsnapped. Calculate delta.
        }
    }
    public static double RotationShortestDistance(double rotation1, double rotation2)
    {
        rotation1 = NormalizeRotation(rotation1);
        rotation2 = NormalizeRotation(rotation2);

        if (rotation1 > rotation2)
        {
            return Math.Min(Math.Abs(rotation1 - rotation2), Math.Abs(rotation2 + 360 - rotation1));
        }
        else
        {
            return Math.Min(Math.Abs(rotation2 - rotation1), Math.Abs(rotation1 + 360 - rotation2));
        }
    }

    public static double NormalizeRotation(double rotation)
    {
        rotation %= 360;

        if (rotation < 0)
        {
            rotation += 360;
        }
        return rotation;
    }
}
