using System;

namespace Mapsui.Projections
{
    public static class DefaultProjectionFactory
    {
        static DefaultProjectionFactory()
        {
            Create = () => throw new Exception("No method to create a projection was registered");
        }

        public static Func<IProjection> Create { get; set; }
    }
}
