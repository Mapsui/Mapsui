using System;

namespace Mapsui.Projections
{
    public class ProjectionDefaults
    {

        /// <summary>
        /// Static property that can be overridden by a user defined IProjection.
        /// </summary>
        public static IProjection Projection { get; set; } = new Projection();

        /// <summary> Get Crs from Esri String </summary>
        public static Func<string, string>? CrsFromEsri { get; set; }
    }
}
