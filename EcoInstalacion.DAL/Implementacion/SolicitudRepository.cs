using EcoInstalacion.DAL.EcoInstaladorContext;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;
using Microsoft.EntityFrameworkCore;

namespace EcoInstalacion.DAL.Implementacion
{
    public class SolicitudRepository : ISolicitudRepository
    {
        private readonly EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public SolicitudRepository(EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<List<SolicitudesInstalacion>> ListaConRelaciones()
        {
            return await _dbContext.SolicitudesInstalacions
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .OrderByDescending(s => s.FechaSolicitud)
                .ThenByDescending(s => s.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<SolicitudesInstalacion?> ObtenerConRelaciones(int id)
        {
            return await _dbContext.SolicitudesInstalacions
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .Include(s => s.VisitasTecnicas)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <inheritdoc/>
        public async Task<SolicitudesInstalacion?> ObtenerConVisitas(int id)
        {
            // Sin AsNoTracking: la entidad debe estar tracked para que
            // GenericRepository.Eliminar pueda ejecutar DbContext.Remove sobre ella.
            return await _dbContext.SolicitudesInstalacions
                .Include(s => s.VisitasTecnicas)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<SolicitudesInstalacion>> ListaPaginadaSinTracking(
            string? q = null,
            string? estado = null,
            int page = 1,
            int pageSize = 25)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedPageSize = pageSize < 1 ? 25 : (pageSize > 100 ? 100 : pageSize);

            var query = _dbContext.SolicitudesInstalacions
                .AsNoTracking()
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(s =>
                    s.Cliente != null &&
                    s.Cliente.NombreCompleto.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(estado) &&
                !estado.Trim().Equals("Todas", StringComparison.OrdinalIgnoreCase))
            {
                var est = estado.Trim();
                query = query.Where(s => s.Estado == est);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.FechaSolicitud)
                .ThenByDescending(s => s.Id)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync();

            return new PagedResult<SolicitudesInstalacion>(
                items,
                normalizedPage,
                normalizedPageSize,
                totalCount);
        }
    }
}
