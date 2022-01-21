namespace Mapsui.Animations;

public interface IAnimatable
{
    /// <returns>Returns true if animations are running and a graphics update is needed.</returns>
    bool UpdateAnimations();
}

