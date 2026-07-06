using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion
{
    public class ReporteService : IReporteService
    {
        private readonly IReporteRepository _reporteRepository;

        public ReporteService(IReporteRepository reporteRepository)
        {
            _reporteRepository = reporteRepository;
        }

        public async Task<List<ReporteTecnicosAsignados>> ReporteTecnicosAsignados(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var solicitudes = await _reporteRepository.SolicitudesConTecnicoYVisitas(fechaInicio, fechaFin);

            var lista = solicitudes
                .SelectMany(
                    s => s.VisitasTecnicas!.DefaultIfEmpty(),
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
