﻿using Mapsui.Nts.Editing;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingAddPolygonSample : ISample
{
    public string Name => "Editing Add Polygon";
    public string Category => "Editing";

    public Task<Map> CreateMapAsync() => Task.FromResult(EditingSample.CreateMap(EditMode.AddPolygon));
}
