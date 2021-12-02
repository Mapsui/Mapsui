namespace Mapsui.Utilities
{
    /// <summary>
    /// Interface for classes, that could be animated
    /// </summary>
    public interface IAnimatable
    {
        /// <summary>
        /// Update possible animations, which belong to the class
        /// </summary>
        /// <param name="ticks">Ticks for calculating the update</param>
        /// <returns>True, if one of the animations need a redraw</returns>
        bool UpdateAnimations(long ticks);
    }
}
