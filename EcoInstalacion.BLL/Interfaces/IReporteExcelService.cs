using System;
using System.Collections.Generic;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IReporteExcelService
    {
        byte[] GenerarReporteTecnicosAsignadosXlsx(
            IReadOnlyList<ReporteTecnicosAsignados> items,
            DateTime? fechaInicio,
            DateTime? fechaFin);
    }
}
