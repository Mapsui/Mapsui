namespace Mapsui;

public class MMinMax
{
    public MMinMax(double value1, double value2)
    {
        if (value1 < value2)
        {
            Min = value1;
            Max = value2;
        }
        else
        {
            Min = value2;
            Max = value1;
        }
    }

    public double Min { get; }
    public double Max { get; }
}
