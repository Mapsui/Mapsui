using System.Collections.Generic;

namespace Mapsui.Providers
{
    public interface IFeatures : IEnumerable<IFeature>
    {
        string PrimaryKey { get; }
        void Add(IFeature feature);
        void Delete(object id);
        void Clear();
    }
}
