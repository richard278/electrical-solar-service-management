using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IReporteService
    {
        Task<List<ReporteTecnicosAsignados>> ReporteTecnicosAsignados(
        DateTime? fechaInicio,
        DateTime? fechaFin);
    }
}
