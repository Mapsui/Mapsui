using Mapsui.Nts.Editing;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingModifySample : ISample
{
    public string Name => "Modify";
    public string Category => "Editing";

    public Task<Map> CreateMapAsync() => Task.FromResult(EditingSample.CreateMap(EditMode.Modify));
}
