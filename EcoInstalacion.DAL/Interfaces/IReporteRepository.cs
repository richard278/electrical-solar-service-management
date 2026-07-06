using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IReporteRepository
    {
        Task<List<SolicitudesInstalacion>> SolicitudesConTecnicoYVisitas(DateTime? fechaInicio, DateTime? fechaFin);
    }
}
