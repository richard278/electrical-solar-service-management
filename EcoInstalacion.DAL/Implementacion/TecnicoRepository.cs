using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
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

        public async Task<EcoInstalacion.Entity.Pagination.PagedResultDto<Tecnico>> ListaPaginadaSinTracking(EcoInstalacion.Entity.Pagination.TecnicoQueryDto query)
        {
            var iquery = _dbContext.Set<Tecnico>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.estado))
            {
                var estadoNorm = query.estado.Trim().ToLowerInvariant();
                if (estadoNorm == "inactivos")
                {
                    iquery = iquery.Where(t => false);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.q))
            {
                // Nota de precisión: EF.Functions.Like() produce un LIKE nativo en SQL Server.
                // La sensibilidad a mayúsculas/minúsculas depende de la collation configurada en SQL Server, base de datos o columna.
                var q = query.q.Trim();
                iquery = iquery.Where(t => EF.Functions.Like(t.Nombre, $"%{q}%") ||
                                           (!string.IsNullOrEmpty(t.Especialidad) && EF.Functions.Like(t.Especialidad, $"%{q}%")) ||
                                           (!string.IsNullOrEmpty(t.Telefono) && EF.Functions.Like(t.Telefono, $"%{q}%")) ||
                                           (!string.IsNullOrEmpty(t.Correo) && EF.Functions.Like(t.Correo, $"%{q}%")));
            }

            if (!string.IsNullOrWhiteSpace(query.nombre))
            {
                var n = query.nombre.Trim();
                iquery = iquery.Where(t => EF.Functions.Like(t.Nombre, $"%{n}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.correo))
            {
                var email = query.correo.Trim();
                iquery = iquery.Where(t => !string.IsNullOrEmpty(t.Correo) && EF.Functions.Like(t.Correo, $"%{email}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.telefono))
            {
                var tel = query.telefono.Trim();
                iquery = iquery.Where(t => !string.IsNullOrEmpty(t.Telefono) && EF.Functions.Like(t.Telefono, $"%{tel}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.especialidad))
            {
                var esp = query.especialidad.Trim();
                iquery = iquery.Where(t => !string.IsNullOrEmpty(t.Especialidad) && EF.Functions.Like(t.Especialidad, $"%{esp}%"));
            }

            var total = await iquery.CountAsync();

            var items = await iquery
                .OrderBy(t => t.Nombre)
                .ThenBy(t => t.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new EcoInstalacion.Entity.Pagination.PagedResultDto<Tecnico>
            {
                Items = items,
                TotalCount = total,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}
