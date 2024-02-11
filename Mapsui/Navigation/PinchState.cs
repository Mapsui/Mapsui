namespace Mapsui;

public record PinchState(MPoint Center, double Radius, double Angle)
{
    public PinchManipulation GetPinchManipulation(PinchState previousPinchState, double totalPinchRotation)
        => new PinchManipulation(Center, previousPinchState.Center, Radius / previousPinchState.Radius, Angle - previousPinchState.Angle, totalPinchRotation);
}
