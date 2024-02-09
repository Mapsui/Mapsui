using System;

namespace Mapsui;

public static class RotationSnapper
{
    /// <summary>
    /// Calculates the rotation delta taking into account the snapping parameters. 
    /// </summary>
    /// <param name="rotation">The rotation of the viewport of the map. This rotation is visible in the map.</param>
    /// <returns></returns>
    public static double AdjustRotationDeltaForSnapping(double rotationDelta, double currentRotation, double virtualRotation, double unSnapRotation, double reSnapRotation)
    {
        if (Math.Abs(currentRotation) < double.Epsilon) // There is no rotation
        {
            if (RotationShortestDistance(virtualRotation, 0) >= unSnapRotation)
                return virtualRotation; // Unsnap. The virtualRotation can be applied.
            else
                return 0; // Still snapped. No delta.
        }
        else // There is rotation
        {
            if (RotationShortestDistance(virtualRotation, 0) <= reSnapRotation)
                return -currentRotation; // Resnap. Undo the actual rotation by returning the inverse as delta. 
            else
                return rotationDelta; // Still unsnapped. Return the rotationDelta unaltered.
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
