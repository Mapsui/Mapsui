namespace Mapsui;

public class MRect2
{
    public MPoint Max { get; }
    public MPoint Min { get; }

    public double MaxX => Max.X;
    public double MaxY => Max.Y;
    public double MinX => Min.X;
    public double MinY => Min.Y;

    public MPoint Centroid => new MPoint(Max.X - Min.X, Max.Y - Min.Y);

    public double Width => Max.X - MinX;
    public double Height => Max.Y - MinY;

    public double Bottom => Min.Y;
    public double Left => Min.X;
    public double Top => Max.Y;
    public double Right => Max.X;

    public MPoint TopLeft => new MPoint(Left, Top);
    public MPoint TopRight => new MPoint(Right, Top);
    public MPoint BottomLeft => new MPoint(Left, Bottom);
    public MPoint BottomRight => new MPoint(Right, Bottom);


    //IEnumerable<MPoint> Vertices { get; }

    //MRect Clone();
    //bool Contains(MPoint? p);
    //bool Contains(MRect r);
    //bool Equals(MRect? other);
    //double GetArea();
    //MRect Grow(double amount);
    //MRect Grow(double amountInX, double amountInY);
    //bool Intersects(MRect? box);
    //MRect Join(MRect? box);
    //MRect Multiply(double factor);
    //MQuad Rotate(double degrees);
}
