using System;
using Mapsui.Layers;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public interface IMyLocationLayer : ILayer
{
    bool IsMoving { get; set; }
    Position MyLocation { get; }
    double Direction { get; }
    string CalloutText { get; set; }
    void UpdateMyDirection(double direction, double viewportRotation, bool animated = false);
    void HandleClicked(IDrawableClicked args);
    void UpdateMyLocation(Position newLocation, bool animated = false);
    event EventHandler<IDrawableClicked> Clicked;
    void UpdateMySpeed(double positionSpeed);
    void UpdateMyViewDirection(double newDirection, double newViewportRotation, bool animated = false);
}
