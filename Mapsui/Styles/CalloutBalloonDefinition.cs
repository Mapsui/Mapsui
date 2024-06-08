namespace Mapsui.Styles;

public record CalloutBalloonDefinition
{
    public double StrokeWidth { get; init; } = 1f;
    public MRect Padding { get; init; } = new(3f, 3f, 3f, 3f);
    public double RectRadius { get; init; } = 4f;
    public TailAlignment TailAlignment { get; init; } = TailAlignment.Bottom;
    public double TailWidth { get; init; } = 8f;
    public double TailHeight { get; init; } = 8f;
    public double TailPosition { get; init; } = 0.5f;
    public double ShadowWidth { get; init; } = 2f;
    public Color BackgroundColor { get; init; } = Color.White;
    public Color Color { get; init; } = Color.Black;
    public Offset Offset { get; init; } = new(0, 0);
}
