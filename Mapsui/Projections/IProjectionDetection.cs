using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Projections;

public interface IProjectionCrs
{
    /// <summary> Get Crs from Esri String </summary>
    public string? CrsFromEsri(string esri);
}
