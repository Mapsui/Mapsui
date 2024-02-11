namespace Mapsui;
public record PinchManipulation(MPoint Center, MPoint PreviousCenter, double ResolutionChange, double RotationChange, double totalRotationChange);
