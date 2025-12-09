using Mapsui.Styles;
using MarinerNotices.MapsuiBuilder.Extensions;
using MarinerNotices.MapsuiBuilder.Utilities;
using MarinerNotices.MapsuiBuilder.Wrappers;
using System;
using System.Collections.Generic;

namespace MarinerNotices.MapsuiBuilder.LayerBuilders;

public class ProjectPointStyleBuilder
{
    private static readonly double _maxVisibleForBoulders = ZoomLevels.GetResolutionBetweenThisAndMoreZoomedOutLevel(16);
    private static readonly double _maxVisibleForDetails = ProjectCenterStyleBuilder.MaxVisibleForDetails;

    private static readonly Dictionary<int, Color> _colors = new()
    {
        // Colors are taken with color picker from images that Robert sent me. -João
		{ 1, Color.FromString("#ef2119") },
        { 2, Color.FromString("#fdae0d") },
        { 3, Color.FromString("#fff157") },
        { 4, Color.FromString("#377E21") },
        { 5, Color.FromString("#1432F5") },
        { 6, Color.FromString("#74F9FD") },
        { 7, Color.FromString("#942192") },
        { 8, Color.FromString("#d5d5d5") },
        { 9, Color.FromString("#000000") },
        { 10, Color.FromString("#ffffff") },
    };

    public static IStyle CreateStyle(ProjectPointWrapper projectPointWrapper)
    {
        return new SymbolStyle();

#pragma warning disable CS0162 // Unreachable code detected
        var symbol = GetSymbol(projectPointWrapper);
#pragma warning restore CS0162 // Unreachable code detected
        if (symbol.MapIcon is null)
        {
            return ImageStyles.CreatePinStyle();
        }

        var imageSource = typeof(VectorTileStyleBuilder).GetImageSourcePath(symbol.MapIcon.ImagePath);

        var imageStyle = new ImageStyle
        {
            Offset = new Offset(0, symbol.MapIcon.OffsetY()),
            SymbolScale = symbol.MapIcon.Scale,
            Opacity = 1f,
            Image = new Image
            {
                Source = imageSource,
                SvgFillColor = symbol.Color,
                RasterizeSvg = true,
            },
            MaxVisible = _maxVisibleForDetails,
        };

        if (projectPointWrapper.Type == 10) // To limit the visibility of boulders. This is because the high number of boulders can affect performance.
            imageStyle.MaxVisible = _maxVisibleForBoulders;

        return imageStyle;
    }

    private static (MapIcon? MapIcon, Color? Color) GetSymbol(BaseWrapper wrapper)
    {
        MapIcon? svgName;
        if (wrapper is ProjectPointWrapper projectPointWrapper)
        {
            svgName = TypeToSvgName(projectPointWrapper.Type);
            _ = _colors.TryGetValue(projectPointWrapper.Color, out var color);
            return (svgName, color);
        }
        else
        {
            throw new Exception($"Expected {nameof(ProjectCenterWrapper)} or {nameof(ProjectPointWrapper)} but got {wrapper.GetType().Name}.");
        }
    }

    private static MapIcon? TypeToSvgName(int? type) => type switch
    {
        // This uses true offset from the center, and only requires the
        // height as the offset is always center and 5 px (units) from bottom.
        1 => new MapIcon(1, "images.mapIcons.EMIN_Icons03.svg", 60),
        2 => new MapIcon(1, "images.mapIcons.EMIN_Icons05.svg", 40),
        3 => new MapIcon(1, "images.mapIcons.EMIN_Icons04.svg", 30),
        4 => new MapIcon(1, "images.mapIcons.EMIN_Icons02.svg", 20),
        5 => new MapIcon(1, "images.mapIcons.EMIN_Icons01.svg", 25),
        6 => new MapIcon(1, "images.mapIcons.EMIN_Icons04.svg", 35),
        7 => new MapIcon(1, "images.mapIcons.EMIN_Icons06.svg", 35),
        10 => new MapIcon(1, "images.mapIcons.EMIN_Icons07.svg", 25),
        _ => null,
    };

    private class MapIcon
    {
        public MapIcon(int id, string imagePath, int heightInPixels, double scale = 1)
        {
            Id = id;
            ImagePath = imagePath;
            HeightInPixels = heightInPixels;
            Scale = scale;
        }

        public int Id { get; set; }

        public string ImagePath { get; set; }

        public int HeightInPixels { get; set; } = 0;

        public int PointHeightInPixels { get; set; } = 5;

        public double Scale { get; set; }

        public double OffsetY()
        {
            return (HeightInPixels / 2) - PointHeightInPixels;
        }
    }
}
