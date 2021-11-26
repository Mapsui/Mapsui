namespace Mapsui.Projection
{
    public class ProjectionDefaults
    {

        /// <summary>
        /// Static property that can be overridden by a user defined IProjection.
        /// </summary>
        public static IProjection Projection { get; set; } = new MinimalProjection();
    }
}
