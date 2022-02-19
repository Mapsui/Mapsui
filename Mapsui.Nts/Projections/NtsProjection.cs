using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Mapsui.Extensions;
using Mapsui.Projections;

namespace Mapsui.Nts.Projections;
public class NtsProjection : IProjection
{
    static NtsProjection()
    {
        DefaultProjectionFactory.Create = () => new NtsProjection();
    }

    private readonly Dictionary<string, string> Projections = new();

    public NtsProjection()
    {
        coordinateSystemFactory = new CoordinateSystemFactory();
        coordinateTransformationFactory = new CoordinateTransformationFactory();

        var assembly = typeof(NtsProjection).Assembly;
        var fullName = assembly.GetFullName("Projections.SRID.csv.gz");
        using var stream = assembly.GetManifestResourceStream(fullName);
        using var gzipStream = new GZipStream(stream!, CompressionMode.Decompress);
        var reader = new StreamReader(gzipStream!, Encoding.UTF8);
        while (reader.ReadLine() is { } line)
        {
            AddProjection(Projections, line);
        }
    }

    private void AddProjection(Dictionary<string, string> projection, string line)
    {
        var split = line.Split(';');
        var key = $"EPSG:{split[0]}";
        projection[key] = split[1];
    }

    public (double X, double Y) Project(string fromCRS, string toCRS, double x, double y)
    {
        throw new NotImplementedException();
    }

    public void Project(string fromCRS, string toCRS, MPoint point)
    {
        throw new NotImplementedException();
    }

    public void Project(string fromCRS, string toCRS, MRect rect)
    {
        throw new NotImplementedException();
    }

    public bool IsProjectionSupported(string? fromCRS, string? toCRS)
    {
        throw new NotImplementedException();
    }

    public void Project(string fromCRS, string toCRS, IFeature feature)
    {
        throw new NotImplementedException();
    }

    public void Project(string fromCRS, string toCRS, IEnumerable<IFeature> features)
    {
        throw new NotImplementedException();
    }
}
