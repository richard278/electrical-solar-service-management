using EcoInstalacion.DAL.EcoInstaladorContext;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
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
        public async Task<EcoInstalacion.Entity.Pagination.PagedResultDto<SolicitudesInstalacion>> ListaPaginadaSinTracking(
            EcoInstalacion.Entity.Pagination.SolicitudQueryDto query)
        {
            var iquery = _dbContext.SolicitudesInstalacions
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.estado) &&
                !query.estado.Trim().Equals("Todas", StringComparison.OrdinalIgnoreCase))
            {
                var est = query.estado.Trim();
                iquery = iquery.Where(s => s.Estado != null && EF.Functions.Like(s.Estado, est));
            }

            if (query.clienteId.HasValue && query.clienteId.Value > 0)
            {
                iquery = iquery.Where(s => s.ClienteId == query.clienteId.Value);
            }

            if (query.tecnicoAsignadoId.HasValue && query.tecnicoAsignadoId.Value > 0)
            {
                iquery = iquery.Where(s => s.TecnicoAsignadoId == query.tecnicoAsignadoId.Value);
            }

            if (query.fechaDesde.HasValue)
            {
                iquery = iquery.Where(s => s.FechaSolicitud >= query.fechaDesde.Value);
            }

            if (query.fechaHasta.HasValue)
            {
                iquery = iquery.Where(s => s.FechaSolicitud <= query.fechaHasta.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.q))
            {
                // Nota de precisión: EF.Functions.Like() produce un LIKE nativo en SQL Server.
                // La sensibilidad a mayúsculas/minúsculas depende de la collation configurada en SQL Server, base de datos o columna.
                var q = query.q.Trim();
                iquery = iquery.Where(s =>
                    (!string.IsNullOrEmpty(s.TipoInstalacion) && EF.Functions.Like(s.TipoInstalacion, $"%{q}%")) ||
                    (!string.IsNullOrEmpty(s.Descripcion) && EF.Functions.Like(s.Descripcion, $"%{q}%")) ||
                    (!string.IsNullOrEmpty(s.Estado) && EF.Functions.Like(s.Estado, $"%{q}%")) ||
                    (s.Cliente != null && EF.Functions.Like(s.Cliente.NombreCompleto, $"%{q}%")) ||
                    (s.TecnicoAsignado != null && EF.Functions.Like(s.TecnicoAsignado.Nombre, $"%{q}%")));
            }

            if (!string.IsNullOrWhiteSpace(query.tipoInstalacion))
            {
                var tipo = query.tipoInstalacion.Trim();
                iquery = iquery.Where(s => !string.IsNullOrEmpty(s.TipoInstalacion) && EF.Functions.Like(s.TipoInstalacion, $"%{tipo}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.clienteNombre))
            {
                var cNom = query.clienteNombre.Trim();
                iquery = iquery.Where(s => s.Cliente != null && EF.Functions.Like(s.Cliente.NombreCompleto, $"%{cNom}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.tecnicoNombre))
            {
                var tNom = query.tecnicoNombre.Trim();
                iquery = iquery.Where(s => s.TecnicoAsignado != null && EF.Functions.Like(s.TecnicoAsignado.Nombre, $"%{tNom}%"));
            }

            var total = await iquery.CountAsync();

            var items = await iquery
                .OrderByDescending(s => s.FechaSolicitud)
                .ThenByDescending(s => s.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new EcoInstalacion.Entity.Pagination.PagedResultDto<SolicitudesInstalacion>
            {
                Items = items,
                TotalCount = total,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}
