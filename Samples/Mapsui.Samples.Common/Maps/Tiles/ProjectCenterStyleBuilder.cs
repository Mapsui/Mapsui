using Mapsui.Styles;
using MarinerNotices.MapsuiBuilder.Extensions;
using MarinerNotices.MapsuiBuilder.Wrappers;
using System;

namespace MarinerNotices.MapsuiBuilder.LayerBuilders;

public class ProjectCenterStyleBuilder
{
    private const double _symbolScale = 0.8;

    // This field should make sure that either ProjectPoint or ProjectCenter is visible, but not both.
    // But I have seen cases where I see both. Perhaps this does not work as expected because it is a
    // style within a ThemeStyle. This would be a Mapsui bug.
    public static double MaxVisibleForDetails { get; } = 866;

    public static IStyle CreateStyle(ProjectCenterWrapper wrapper)
    {
        return new SymbolStyle();

#pragma warning disable CS0162 // Unreachable code detected
        if (wrapper.ClusterSize > 1)
        {
            return new StyleCollection()
            {
                Styles =
                [
                    new SymbolStyle
                    {
                        SymbolType = SymbolType.Ellipse,
                        SymbolScale = 1.5625 * _symbolScale,
                        Opacity = 0.6f,
                    },
                    new LabelStyle()
                    {
                        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
                        Text = wrapper.ClusterSize.ToString(),
                        Font = new Font { Size = 20 },
                        BackColor = new Brush(Color.Transparent),
                    },
                ],
            };
        }
#pragma warning restore CS0162 // Unreachable code detected

        var imagePath = GetImagePath(wrapper);
        var imageSource = typeof(VectorTileStyleBuilder).GetImageSourcePath(imagePath);

        var imageStyle = new ImageStyle
        {
            Opacity = 1f,
            Image = new Image
            {
                Source = imageSource,
                RasterizeSvg = true,
            },
            MinVisible = MaxVisibleForDetails,
            SymbolScale = _symbolScale,
        };
        return imageStyle;
    }

    private static string GetImagePath(BaseWrapper wrapper)
    {
        if (wrapper is not ProjectCenterWrapper projectCenterWrapper)
            throw new Exception($"Expected {nameof(ProjectCenterWrapper)} or {nameof(ProjectPointWrapper)} but got {wrapper.GetType().Name}.");
        else
            return ProjectTypeToSvgName(projectCenterWrapper.Type);
    }

    private static string ProjectTypeToSvgName(int? projectType) => projectType switch
    {
        // This uses true offset from the center, and only requires the
        // height as the offset is always center and 5 px (units) from bottom.
        1 => "images.mapIcons.Project_Types-01.svg",
        2 => "images.mapIcons.Project_Types-02.svg",
        3 => "images.mapIcons.Project_Types-03.svg",
        4 => "images.mapIcons.Project_Types-04.svg",
        5 => "images.mapIcons.Project_Types-05.svg",
        6 => "images.mapIcons.Project_Types-06.svg",
        7 => "images.mapIcons.Project_Types-07.svg",
        _ => throw new NotImplementedException(),
    };
}
