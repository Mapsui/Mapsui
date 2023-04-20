using Mapsui.Layers;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public interface IMyLocationLayer : ILayer
{
    bool IsMoving { get; set; }
    Position MyLocation { get; }
    double Direction { get; }
    void UpdateMyDirection(double direction, double viewportRotation, bool animated = false);
}
