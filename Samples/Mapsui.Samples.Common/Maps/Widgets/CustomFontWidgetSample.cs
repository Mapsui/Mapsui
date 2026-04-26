using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.InfoWidgets;
using Mapsui.Widgets.ScaleBar;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

/// <summary>
/// Demonstrates custom font loading via <see cref="Font.FontSource"/> on every widget renderer
/// that was updated to support it: TextBoxWidget, LoggingWidget, PerformanceWidget,
/// GridLayer coordinate labels, and ScaleBarWidget.
/// </summary>
public class CustomFontWidgetSample : ISample
{
    public string Name => "Custom Font Widgets";
    public string Category => "Widgets";

    // Noto Sans Arabic covers Arabic script and is licensed under OFL.
    private const string NotoSansArabicSource = "embedded://Mapsui.Samples.Common.Resources.Fonts.NotoSansArabic-Regular.ttf";
    // Noto Sans SC covers Simplified Chinese and is licensed under OFL.
    private const string NotoSansSCSource = "embedded://Mapsui.Samples.Common.Resources.Fonts.NotoSansSC-Regular.ttf";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();

        // GridLayer — coordinate labels rendered with custom font.
        map.Layers.Add(new GridLayer
        {
            LabelFont = new Font { Size = 12, FontSource = NotoSansArabicSource },
        });

        map.Navigator.CenterOnAndZoomTo(new MPoint(0, 0), 2500);

        // Arabic TextBoxWidget — demonstrates a custom font (NotoSansArabic) via FontSource.
        map.Widgets.Add(new TextBoxWidget
        {
            Text = "مرحبا بالعالم",
            Font = new Font { Size = 13, FontSource = NotoSansArabicSource },
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new MRect(10),
            Padding = new MRect(6),
            CornerRadius = 4,
            BackColor = new Color(60, 80, 120),
            TextColor = Color.White,
        });

        // LoggingWidget — log output rendered with custom font.
        var loggingWidget = new LoggingWidget(map.RefreshGraphics)
        {
            Font = new Font { Size = 11, FontSource = NotoSansArabicSource },
            BackColor = Color.White,
            Opacity = 0.9f,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new MRect(10),
            Padding = new MRect(4),
            Width = 320,
            Height = 90,
        };
        map.Widgets.Add(loggingWidget);

        // PerformanceWidget — fps readout rendered with custom font.
        var performanceWidget = map.Widgets.OfType<PerformanceWidget>().First();
        performanceWidget.Font = new Font { Size = 11, FontSource = NotoSansArabicSource };
        performanceWidget.Performance.IsActive = ActiveMode.Yes;
        performanceWidget.BackColor = new Color(40, 40, 40);
        performanceWidget.TextColor = Color.White;
        performanceWidget.Opacity = 1;

        // Chinese TextBoxWidget — bottom-left; uses embedded NotoSansSC for CJK.
        map.Widgets.Add(new TextBoxWidget
        {
            Text = "欢迎使用 Mapsui",
            Font = new Font { Size = 13, FontSource = NotoSansSCSource },
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new MRect(10),
            Padding = new MRect(6),
            CornerRadius = 4,
            BackColor = new Color(80, 60, 120),
            TextColor = Color.White,
        });

        // ScaleBarWidget — tick labels rendered with custom font, centered at the bottom.
        map.Widgets.Add(new ScaleBarWidget(map)
        {
            Font = new Font { Size = 11, FontSource = NotoSansArabicSource },
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new MRect(10),
        });

        Logger.Log(LogLevel.Information, "CustomFontWidgetSample loaded – all widgets use NotoSansArabic via FontSource", null);

        return map;
    }
}

