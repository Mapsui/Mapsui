namespace Mapsui.Projection
{
    public class ProjectionDefaults
    {

        /// <summary>
        /// Static property that can be overridden by a user defined ITransformation.
        /// </summary>
        public static ITransformation Transformation { get; set; } = new MinimalTransformation();
    }
}
