using SharpMap.Data;
using System.Data;

namespace SharpMap.Providers
{
    static class Utilities
    {
        public static IFeatures DataSetToFeatures(FeatureDataSet dataSet)
        {
            IFeatures features = new Features();

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
