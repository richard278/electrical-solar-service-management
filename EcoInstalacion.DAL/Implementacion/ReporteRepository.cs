using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.EntityFrameworkCore;

namespace EcoInstalacion.DAL.Implementacion
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly EcoInstalacion.DAL.EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public ReporteRepository(EcoInstalacion.DAL.EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SolicitudesInstalacion>> SolicitudesConTecnicoYVisitas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            return await _dbContext.Set<SolicitudesInstalacion>()
                .AsNoTracking()
                .Include(s => s.TecnicoAsignado)
                .Include(s => s.VisitasTecnicas)
                .Where(s =>
                    (!fechaInicio.HasValue || s.FechaSolicitud >= fechaInicio.Value.Date) &&
                    (!fechaFin.HasValue || s.FechaSolicitud <= fechaFin.Value.Date.AddDays(1).AddTicks(-1)))
                .ToListAsync();
        }
    }
}
