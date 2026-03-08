using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IReportePdfService
    {
        byte[] GenerarReporteTecnicosAsignadosPDF(
            IList<ReporteTecnicosAsignados> items,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            byte[]? logoBytes = null
        );
    }
}
