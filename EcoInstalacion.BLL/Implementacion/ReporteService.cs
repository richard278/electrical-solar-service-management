using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.EntityFrameworkCore;

namespace EcoInstalacion.BLL.Implementacion
{
    public class ReporteService : IReporteService
    {
        private readonly IGenericRepository<SolicitudesInstalacion> _repoSolicitud;
        private readonly IGenericRepository<VisitasTecnica> _repoVisita;
        private readonly IGenericRepository<Tecnico> _repoTecnico;
        public ReporteService(
            IGenericRepository<SolicitudesInstalacion> repoSolicitud,
            IGenericRepository<VisitasTecnica> repoVisita,
            IGenericRepository<Tecnico> repoTecnico)
        {
            _repoSolicitud = repoSolicitud;
            _repoVisita = repoVisita;
            _repoTecnico = repoTecnico;
        }

        public async Task<List<ReporteTecnicosAsignados>> ReporteTecnicosAsignados(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var solicitudes = await _repoSolicitud.ConsultarIncluyendo(
                include: q => q
                    .Include(s => s.TecnicoAsignado)
                    .Include(s => s.VisitasTecnicas),
                filtro: s =>
                    (!fechaInicio.HasValue || s.FechaSolicitud >= fechaInicio.Value.Date) &&
                    (!fechaFin.HasValue || s.FechaSolicitud <= fechaFin.Value.Date.AddDays(1).AddTicks(-1))
            );

            // 2) Transformar cada solicitud + cada visita en fila de reporte
            var lista = solicitudes
                .SelectMany(
                    s => s.VisitasTecnicas!.DefaultIfEmpty(),   // el ! dice al compilador “confía, no es null en tiempo de ejecución”
                    (s, v) => new ReporteTecnicosAsignados
                    {
                        Tecnico = s.TecnicoAsignado?.Nombre ?? "(Sin técnico)",
                        Especialidad = s.TecnicoAsignado?.Especialidad ?? "",
                        FechaSolicitud = s.FechaSolicitud,
                        FechaVisita = v?.FechaVisita,
                        TipoInstalacion = s.TipoInstalacion,
                        Descripcion = s.Descripcion,
                        Estado = s.Estado,
                        Observaciones = v?.Observaciones ?? "",
                        Aprobado = v?.Aprobado ?? false
                    })
                .OrderBy(r => r.FechaSolicitud)
                .ThenBy(r => r.FechaVisita)
                .ToList();

            return lista;
        }
    }
}
