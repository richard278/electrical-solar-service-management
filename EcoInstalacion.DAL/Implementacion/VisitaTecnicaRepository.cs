using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.EntityFrameworkCore;
using EcoInstalacion.DAL.EcoInstaladorContext;

namespace EcoInstalacion.DAL.Implementacion
{
    public class VisitaTecnicaRepository : IVisitaTecnicaRepository
    {
        private readonly EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public VisitaTecnicaRepository(EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<VisitasTecnica>> ListaConSolicitudCliente()
        {
            return await _dbContext.Set<VisitasTecnica>()
                .AsNoTracking()
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .OrderByDescending(v => v.FechaVisita)
                .ToListAsync();
        }

        public async Task<List<VisitasTecnica>> ListaPorSolicitudConSolicitudCliente(int solicitudId)
        {
            return await _dbContext.Set<VisitasTecnica>()
                .AsNoTracking()
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .Where(v => v.SolicitudId == solicitudId)
                .OrderByDescending(v => v.FechaVisita)
                .ToListAsync();
        }

        public async Task<VisitasTecnica?> ObtenerPorIdConSolicitudCliente(int id)
        {
            return await _dbContext.Set<VisitasTecnica>()
                .AsNoTracking()
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
    }
}
