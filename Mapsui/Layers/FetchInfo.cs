namespace Mapsui.Layers;

public class FetchInfo
{
    public FetchInfo(MSection section, string? crs = null, ChangeType changeType = ChangeType.Discrete)
    {
        Section = section;
        CRS = crs;
        ChangeType = changeType;
    }

    public FetchInfo(FetchInfo fetchInfo)
    {
        Section = fetchInfo.Section;
        CRS = fetchInfo.CRS;
        ChangeType = fetchInfo.ChangeType;
    }

    public MSection Section { get; }

    public MRect Extent => Section.Extent;
    public double Resolution => Section.Resolution;
    public string? CRS { get; }
    public ChangeType ChangeType { get; }

    public override bool Equals(object? obj)
    {
        if (obj is FetchInfo fetchInfo)
            return SectionEquals(Section, fetchInfo.Section) && CRS == fetchInfo.CRS && ChangeType == fetchInfo.ChangeType;

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    private bool SectionEquals(MSection section, MSection otherSection)
    {
        // I assume the section.Equals method does an instance compare on Extent, so I added this custom compare.
        // Todo: write a test for this and perhaps use a record struct for section.
        return section.Extent.Equals(otherSection.Extent) && section.Resolution == otherSection.Resolution;
    }

    public FetchInfo Grow(double amountInScreenUnits)
    {
        var amount = amountInScreenUnits * 2 * Resolution;
        return new FetchInfo(new MSection(Section.Extent.Grow(amount), Resolution), CRS, ChangeType);
    }
}
