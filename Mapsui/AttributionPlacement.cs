using Mapsui.Styles;

namespace Mapsui
{
    public class AttributionPlacement
    {
        public LabelStyle.HorizontalAlignmentEnum HorizontalAlignment { get; set; } = LabelStyle.HorizontalAlignmentEnum.Right;
        public LabelStyle.VerticalAlignmentEnum VerticalAlignment { get; set; } = LabelStyle.VerticalAlignmentEnum.Bottom;
        public Offset Offset { get; } = new Offset(4, 4);
    }
}
