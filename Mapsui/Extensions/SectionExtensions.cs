namespace Mapsui.Extensions;
public static class SectionExtensions
{
    public static MSection Multiply(this MSection section, double extentMultiplier = 1, double resolutionMultiplier = 1)
    {
        return new MSection(section.Extent.Multiply(extentMultiplier), section.Resolution / resolutionMultiplier);
    }
}
