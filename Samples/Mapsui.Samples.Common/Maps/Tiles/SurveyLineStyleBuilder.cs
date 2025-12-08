using Mapsui.Styles;
using MarinerNotices.MapsuiBuilder.Wrappers;
using System.Collections.Generic;

namespace MarinerNotices.MapsuiBuilder.LayerBuilders;

public class SurveyLineStyleBuilder
{
    private static readonly VectorStyle _defaultStyle = BuildStyle("#95A5A6"); // Gray - fallback
    private static readonly Dictionary<int, VectorStyle> _styles = new()
    {
        { 1, BuildStyle("#9B59B6") }, // Purple - HVDC
		{ 2, BuildStyle("#E74C3C") }, // Red - HVAC
		{ 3, BuildStyle("#2ECC71") }, // Green - inter-array
		{ 4, BuildStyle("#2C3E50") }, // Black/Dark Gray - Telecoms
		{ 5, BuildStyle("#95A5A6") }, // Gray - Planned Route
	};

    public static VectorStyle CreateStyle(SurveyLineWrapper surveyLineWrapper)
    {
        if (_styles.TryGetValue(surveyLineWrapper.Type, out var style))
            return style;

        return _defaultStyle;
    }

    private static VectorStyle BuildStyle(string hex) =>
        new()
        {
            Line = new Pen(Color.FromString(hex), 3)
            {
                PenStyle = PenStyle.Solid,
            },
        };
}
