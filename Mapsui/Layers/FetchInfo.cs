using System.Threading;

namespace Mapsui.Layers;

public class FetchInfo
{
    public FetchInfo(MSection section, string? crs = null, ChangeType changeType = ChangeType.Discrete, CancellationToken? cancellationToken = null)
    {
        Section = section;
        CRS = crs;
        ChangeType = changeType;
        CancellationToken = cancellationToken;
    }

    public FetchInfo(FetchInfo fetchInfo)
    {
        Section = fetchInfo.Section;
        CRS = fetchInfo.CRS;
        ChangeType = fetchInfo.ChangeType;
        CancellationToken = fetchInfo.CancellationToken;
    }

    public MSection Section { get; }

    public MRect Extent => Section.Extent;
    public double Resolution => Section.Resolution;
    public string? CRS { get; }
    public ChangeType ChangeType { get; }
    public CancellationToken? CancellationToken { get; }

    public FetchInfo Grow(double amountInScreenUnits)
    {
        var amount = amountInScreenUnits * 2 * Resolution;
        return new FetchInfo(new MSection(Section.Extent.Grow(amount), Resolution), CRS, ChangeType, CancellationToken);
    }
}
