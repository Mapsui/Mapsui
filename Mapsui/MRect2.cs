namespace Mapsui;

public class MRect2
{

    public MPoint Max { get; }
    public MPoint Min { get; }

    public double MaxX => Max.X;
    public double MaxY => Max.Y;
    public double MinX => Min.X;
    public double MinY => Min.Y;

    MPoint Centroid => new MPoint(Max.X - Min.X, Max.Y - Min.Y);

    double Width => Max.X - MinX;
    double Height => Max.Y - MinY;

    double Bottom => Min.Y;
    double Left => Min.X;
    double Top => Max.Y;
    double Right => Max.X;

    MPoint TopLeft => new MPoint(Left, Top);
    MPoint TopRight => new MPoint(Right, Top);
    MPoint BottomLeft => new MPoint(Left, Bottom);
    MPoint BottomRight => new MPoint(Right, Bottom);


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
