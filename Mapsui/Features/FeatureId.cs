using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Features;
public static class FeatureId
{
    public static long CreateId(int layerId, uint featureId)
    {
        // Encode the layer featureId and the feature featureId into a single long by using the negative values of long.
        var result = -1 * uint.MaxValue * layerId - featureId;
        return result;
    }

    // Map other types to uint
    public static long CreateId<T>(int layerId, T key, Func<T, uint> keyCreator)
    {
        return CreateId(layerId, keyCreator(key));
    }
}
