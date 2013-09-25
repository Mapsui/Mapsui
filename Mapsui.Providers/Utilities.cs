using System.Collections.Generic;
using System.Data;

namespace Mapsui.Providers
{
    static class Utilities
    {
        public static IEnumerable<IFeature> DataSetToFeatures(FeatureDataTable table)
        {
            var features = new Features();

            foreach (FeatureDataRow row in table)
            {
                IFeature feature = features.New();
                feature.Geometry = row.Geometry;
                foreach (DataColumn column in table.Columns)
                    feature[column.ColumnName] = row[column.ColumnName];

                features.Add(feature);
            }
            return features;
        }
    }
}
