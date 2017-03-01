using Mapsui.Data;
using System.Data;

namespace Mapsui.Providers
{
    static class Utilities
    {
        public static IEnumerable<IFeature> DataSetToFeatures(FeatureDataSet dataSet)
        {
            var features = new Features();

            foreach (FeatureDataTable table in dataSet.Tables)
            {
                foreach (FeatureDataRow row in table)
                {
                    IFeature feature = features.New();
                    feature.Geometry = row.Geometry;
                    foreach (DataColumn column in table.Columns)
                        feature[column.ColumnName] = row[column.ColumnName];

                    features.Add(feature);
                }
            }
            return features;
        }
    }
}

