using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;
using Microsoft.EntityFrameworkCore;
using EcoInstalacion.DAL.EcoInstaladorContext;

namespace EcoInstalacion.DAL.Implementacion
{
    public class TecnicoRepository : ITecnicoRepository
    {
        private readonly EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public TecnicoRepository(EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> EstaAsignadoASolicitudes(int tecnicoId)
        {
            return await _dbContext.Set<SolicitudesInstalacion>()
                .AnyAsync(s => s.TecnicoAsignadoId == tecnicoId);
        }

        public async Task<List<Tecnico>> ListaSinTracking()
        {
            return await _dbContext.Set<Tecnico>()
                .AsNoTracking()
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<Tecnico?> ObtenerPorIdSinTracking(int id)
        {
            return await _dbContext.Set<Tecnico>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<PagedResult<Tecnico>> ListaPaginadaSinTracking(
            string? q = null,
            int page = 1,
            int pageSize = 25)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedPageSize = pageSize < 1 ? 25 : (pageSize > 100 ? 100 : pageSize);

            var query = _dbContext.Set<Tecnico>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(t =>
                    t.Nombre.Contains(term) ||
                    (t.Especialidad != null && t.Especialidad.Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.Nombre)
                .ThenBy(t => t.Id)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync();

            return new PagedResult<Tecnico>(
                items,
                normalizedPage,
                normalizedPageSize,
                totalCount);
        }
    }
}
