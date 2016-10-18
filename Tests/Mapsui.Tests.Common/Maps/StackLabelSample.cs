using Mapsui.Layers;

namespace Mapsui.Tests.Common.Maps
{
    public static class StackLabelSample
    {
        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                //!!!DataSource = CreateProviderWithLabels(),
                Name = "Labels"
            });

            return map;
        }
    }
}
