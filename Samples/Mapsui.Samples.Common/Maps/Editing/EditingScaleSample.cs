using Mapsui.Nts.Editing;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingScaleSample : ISample
{
    public string Name => "Scale";
    public string Category => "Editing";

    public Task<Map> CreateMapAsync() => Task.FromResult(EditingSample.CreateMap(EditMode.Scale));
}
