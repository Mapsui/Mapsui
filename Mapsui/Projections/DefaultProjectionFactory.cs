using System;

namespace Mapsui.Projections
{
    public static class DefaultProjectionFactory
    {
        static DefaultProjectionFactory()
        {
            // Default Projection
            Create = () => new Projection();
        }

        public static Func<IProjection> Create { get; set; }
    }
}
