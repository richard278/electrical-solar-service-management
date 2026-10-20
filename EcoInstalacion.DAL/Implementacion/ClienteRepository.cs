using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;
using Microsoft.EntityFrameworkCore;
using EcoInstalacion.DAL.EcoInstaladorContext;

namespace EcoInstalacion.DAL.Implementacion
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public ClienteRepository(EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> TieneSolicitudes(int clienteId)
        {
            return await _dbContext.Set<SolicitudesInstalacion>()
                .AnyAsync(s => s.ClienteId == clienteId);
        }

        public async Task<List<Cliente>> ListaActivosSinTracking()
        {
            return await _dbContext.Set<Cliente>()
                .AsNoTracking()
                .Where(c => c.EsActivo)
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();
        }

        public async Task<List<Cliente>> ListaPorEstadoSinTracking(bool? esActivo)
        {
            var query = _dbContext.Set<Cliente>().AsNoTracking();

            if (esActivo.HasValue)
                query = query.Where(c => c.EsActivo == esActivo.Value);

            return await query
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Cliente?> ObtenerPorIdSinTracking(int id)
        {
            return await _dbContext.Set<Cliente>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<PagedResult<Cliente>> ListaPaginadaSinTracking(
            string? q = null,
            bool? esActivo = null,
            int page = 1,
            int pageSize = 25)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedPageSize = pageSize < 1 ? 25 : (pageSize > 100 ? 100 : pageSize);

            var query = _dbContext.Set<Cliente>().AsNoTracking();

            if (esActivo.HasValue)
            {
                query = query.Where(c => c.EsActivo == esActivo.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    c.NombreCompleto.Contains(term) ||
                    c.Dui.Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.NombreCompleto)
                .ThenBy(c => c.Id)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync();

            return new PagedResult<Cliente>(
                items,
                normalizedPage,
                normalizedPageSize,
                totalCount);
        }
    }
}
