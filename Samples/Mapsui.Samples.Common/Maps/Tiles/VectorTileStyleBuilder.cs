using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using MarinerNotices.MapsuiBuilder.Wrappers;
using System;
using System.Collections.Generic;

namespace MarinerNotices.MapsuiBuilder.LayerBuilders;

public class VectorTileStyleBuilder
{
    private static readonly Dictionary<string, IStyle> _cache = [];

    public static ThemeStyle CreateStyle()
    {
        return new ThemeStyle((f) =>
        {
            if (f.Data is null)
            {
                if (f["featureType"]?.ToString() == "project")
                    f.Data = new ProjectCenterWrapper((GeometryFeature)f);
                else if (f["featureType"]?.ToString() == "project-point")
                    f.Data = new ProjectPointWrapper((GeometryFeature)f);
                else if (f["featureType"]?.ToString() == "survey-line")
                    f.Data = new SurveyLineWrapper((GeometryFeature)f);
                else if (f["featureType"]?.ToString() == "boundary-area")
                    f.Data = new BoundaryAreaWrapper((GeometryFeature)f);
                else
                    throw new Exception($"Unknown featureType '{f["featureType"]}'");
            }

            if (f.Data is BaseWrapper wrapper)
            {
                var cacheKey = wrapper.GetSymbolStyleKey();
                if (_cache.TryGetValue(cacheKey, out var value))
                    return value;

                IStyle symbolStyle;
                if (wrapper is ProjectCenterWrapper projectCenterWrapper)
                    symbolStyle = ProjectCenterStyleBuilder.CreateStyle(projectCenterWrapper);
                else if (wrapper is ProjectPointWrapper projectPointWrapper)
                    symbolStyle = ProjectPointStyleBuilder.CreateStyle(projectPointWrapper);
                else if (wrapper is SurveyLineWrapper surveyLineWrapper)
                    symbolStyle = SurveyLineStyleBuilder.CreateStyle(surveyLineWrapper);
                else if (wrapper is BoundaryAreaWrapper boundaryAreaWrapper)
                    symbolStyle = BoundaryAreaStyleBuilder.CreateStyle(boundaryAreaWrapper);
                else
                    throw new Exception($"Unexpected type: '{wrapper.GetType().Name}'.");

                _cache[cacheKey] = symbolStyle;

                return symbolStyle;
            }

            throw new Exception($"Expected {nameof(BaseWrapper)}");
        });
    }
}
