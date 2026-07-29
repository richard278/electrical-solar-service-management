using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
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

        public async Task<EcoInstalacion.Entity.Pagination.PagedResultDto<Cliente>> ListaPaginadaSinTracking(EcoInstalacion.Entity.Pagination.ClienteQueryDto query, bool? fuerzaActivo = null)
        {
            var iquery = _dbContext.Set<Cliente>().AsNoTracking();

            if (fuerzaActivo.HasValue)
            {
                iquery = iquery.Where(c => c.EsActivo == fuerzaActivo.Value);
            }
            else if (!string.IsNullOrWhiteSpace(query.estado))
            {
                var estadoNorm = query.estado.Trim().ToLowerInvariant();
                if (estadoNorm == "activos") iquery = iquery.Where(c => c.EsActivo);
                else if (estadoNorm == "inactivos") iquery = iquery.Where(c => !c.EsActivo);
            }

            if (!string.IsNullOrWhiteSpace(query.q))
            {
                // Nota de precisión: EF.Functions.Like() produce un LIKE nativo en SQL Server.
                // La sensibilidad a mayúsculas/minúsculas depende de la collation configurada en SQL Server, base de datos o columna.
                var q = query.q.Trim();
                iquery = iquery.Where(c => EF.Functions.Like(c.NombreCompleto, $"%{q}%") || 
                                           EF.Functions.Like(c.Dui, $"%{q}%") || 
                                           (!string.IsNullOrEmpty(c.Correo) && EF.Functions.Like(c.Correo, $"%{q}%")));
            }

            if (!string.IsNullOrWhiteSpace(query.nombre))
            {
                var n = query.nombre.Trim();
                iquery = iquery.Where(c => EF.Functions.Like(c.NombreCompleto, $"%{n}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.correo))
            {
                var email = query.correo.Trim();
                iquery = iquery.Where(c => !string.IsNullOrEmpty(c.Correo) && EF.Functions.Like(c.Correo, $"%{email}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.dui))
            {
                var dui = query.dui.Trim();
                iquery = iquery.Where(c => EF.Functions.Like(c.Dui, $"%{dui}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.telefono))
            {
                var tel = query.telefono.Trim();
                iquery = iquery.Where(c => !string.IsNullOrEmpty(c.Telefono) && EF.Functions.Like(c.Telefono, $"%{tel}%"));
            }

            var total = await iquery.CountAsync();

            var items = await iquery
                .OrderBy(c => c.NombreCompleto)
                .ThenBy(c => c.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new EcoInstalacion.Entity.Pagination.PagedResultDto<Cliente>
            {
                Items = items,
                TotalCount = total,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}
