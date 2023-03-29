namespace Mapsui.Animations;

/// <typeparam name="T">The type that the animation is changing over time.</typeparam>
/// <param name="CurrentState">The state of the object that is animated after the current update.</param>
/// <param name="IsRunning">Will be false if the animation is cancelled.</param>
public record struct AnimationResult<T>(T State, bool IsRunning);
