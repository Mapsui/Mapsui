using Mapsui.Nts.Editing;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingAddPointSample : ISample
{
    public string Name => "Editing Add Point";
    public string Category => "Editing";

    public Task<Map> CreateMapAsync() => Task.FromResult(EditingSample.CreateMap(EditMode.AddPoint));
}
