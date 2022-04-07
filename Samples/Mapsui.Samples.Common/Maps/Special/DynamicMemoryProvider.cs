using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Providers;
using System.Collections.Generic;

namespace Mapsui.Samples.Common.Maps.Special
{
    internal class DynamicMemoryProvider : MemoryProvider<IFeature>, IDynamic
    {
        public event DataChangedEventHandler? DataChanged;

        public void DataHasChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs());
        }

        public void Add(IFeature feature)
        {
            var list = (List<IFeature>)Features;
            list.Add(feature);
            Features = list;
        }
    }
}
