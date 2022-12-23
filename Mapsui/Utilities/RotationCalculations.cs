using System;

namespace Mapsui.Utilities;

public static class RotationCalculations
{
    public static double NormalizeRotation(double rotation)
    {
        rotation %= 360;

        if (rotation < 0)
        {
            rotation += 360;
        }
        return rotation;
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

    /// <summary>
    /// Calculates the rotation delta taking into account the snapping parameters. 
    /// </summary>
    /// <param name="virtualRotation">The virtual rotation that is determined by the users rotating pinch behavior.</param>
    /// <param name="actualRotation">The rotation of the viewport of the map. This rotation is visible in the map.</param>
    /// <param name="unSnapRotation">The minimal value of the virtual rotation needed to unsnap (allow rotation other than 0).</param>
    /// <param name="reSnapRotation">If the virtual rotation is below this value the map will resnap (rotation will be set to 0).</param>
    /// <returns></returns>
    public static double CalculateRotationDeltaWithSnapping(double virtualRotation, double actualRotation, double unSnapRotation, double reSnapRotation)
    {
        if (Math.Abs(actualRotation) < double.Epsilon) // There is no rotation
        {
            if (RotationShortestDistance(virtualRotation, 0) >= unSnapRotation)
                return virtualRotation; // Unsnap. The vitualRotation can be applied.
            else
                return 0; // Still snapped. No delta.
        }
        else // There is rotation
        {
            if (RotationShortestDistance(virtualRotation, 0) <= reSnapRotation)
                return -actualRotation; // Resnap. Undo the actual rotation by returning the inverse as delta. 
            else
                return virtualRotation - actualRotation; // Still unsnapped. Calculate delta.
        }
    }
}
