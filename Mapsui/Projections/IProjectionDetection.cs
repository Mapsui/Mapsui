namespace Mapsui.Projections;

public interface IProjectionCrs
{
    /// <summary> Get Crs from Esri String </summary>
    public string? CrsFromEsri(string esri);
}
