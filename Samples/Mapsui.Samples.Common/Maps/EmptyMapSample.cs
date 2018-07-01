using Mapsui.Geometries;

namespace Mapsui.Samples.Common.Maps
{
    public static class EmptyMapSample
    {
        public static Map CreateMap()
        {
            return new Map
            {
                Home = n => n.NavigateTo(new BoundingBox(-180, -90, 180, 90))
            };
        }
    }
}