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
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.TecnicoAsignado)
                .OrderByDescending(v => v.FechaVisita)
                .ToListAsync();
        }

        public async Task<List<VisitasTecnica>> ListaPorSolicitudConSolicitudCliente(int solicitudId)
        {
            return await _dbContext.Set<VisitasTecnica>()
                .AsNoTracking()
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.TecnicoAsignado)
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
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.TecnicoAsignado)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> EditarConTransicionAprobacion(
            VisitasTecnica visita,
            bool actualizarSolicitudAEnProceso)
        {
            if (actualizarSolicitudAEnProceso)
            {
                var solicitud = await _dbContext.Set<SolicitudesInstalacion>()
                    .FirstOrDefaultAsync(s => s.Id == visita.SolicitudId);

                if (solicitud is null)
                    throw new InvalidOperationException("La solicitud asociada no existe.");

                solicitud.Estado = EstadosSolicitudSistema.EnProceso;
            }

            _dbContext.Update(visita);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<EcoInstalacion.Entity.Pagination.PagedResultDto<VisitasTecnica>> ListaPaginadaSinTracking(
            EcoInstalacion.Entity.Pagination.VisitaTecnicaQueryDto query)
        {
            var iquery = _dbContext.Set<VisitasTecnica>()
                .AsNoTracking()
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.TecnicoAsignado)
                .AsQueryable();

            if (query.aprobado.HasValue)
            {
                var isAprobado = query.aprobado.Value;
                iquery = isAprobado
                    ? iquery.Where(v => v.Aprobado == true)
                    : iquery.Where(v => v.Aprobado == false || v.Aprobado == null);
            }

            if (query.solicitudId.HasValue && query.solicitudId.Value > 0)
            {
                iquery = iquery.Where(v => v.SolicitudId == query.solicitudId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.estadoSolicitud) &&
                !query.estadoSolicitud.Trim().Equals("Todas", StringComparison.OrdinalIgnoreCase))
            {
                var est = query.estadoSolicitud.Trim();
                iquery = iquery.Where(v => v.Solicitud != null && v.Solicitud.Estado != null && EF.Functions.Like(v.Solicitud.Estado, est));
            }

            if (query.fechaDesde.HasValue)
            {
                iquery = iquery.Where(v => v.FechaVisita >= query.fechaDesde.Value);
            }

            if (query.fechaHasta.HasValue)
            {
                iquery = iquery.Where(v => v.FechaVisita <= query.fechaHasta.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.q))
            {
                // Nota de precisión: EF.Functions.Like() produce un LIKE nativo en SQL Server.
                // La sensibilidad a mayúsculas/minúsculas depende de la collation configurada en SQL Server, base de datos o columna.
                var q = query.q.Trim();
                iquery = iquery.Where(v =>
                    (!string.IsNullOrEmpty(v.Observaciones) && EF.Functions.Like(v.Observaciones, $"%{q}%")) ||
                    (v.Solicitud != null && !string.IsNullOrEmpty(v.Solicitud.TipoInstalacion) && EF.Functions.Like(v.Solicitud.TipoInstalacion, $"%{q}%")) ||
                    (v.Solicitud != null && !string.IsNullOrEmpty(v.Solicitud.Estado) && EF.Functions.Like(v.Solicitud.Estado, $"%{q}%")) ||
                    (v.Solicitud != null && v.Solicitud.Cliente != null && EF.Functions.Like(v.Solicitud.Cliente.NombreCompleto, $"%{q}%")) ||
                    (v.Solicitud != null && v.Solicitud.TecnicoAsignado != null && EF.Functions.Like(v.Solicitud.TecnicoAsignado.Nombre, $"%{q}%")));
            }

            if (!string.IsNullOrWhiteSpace(query.clienteNombre))
            {
                var cNom = query.clienteNombre.Trim();
                iquery = iquery.Where(v => v.Solicitud != null && v.Solicitud.Cliente != null && EF.Functions.Like(v.Solicitud.Cliente.NombreCompleto, $"%{cNom}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.tecnicoNombre))
            {
                var tNom = query.tecnicoNombre.Trim();
                iquery = iquery.Where(v => v.Solicitud != null && v.Solicitud.TecnicoAsignado != null && EF.Functions.Like(v.Solicitud.TecnicoAsignado.Nombre, $"%{tNom}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.tipoInstalacion))
            {
                var tipo = query.tipoInstalacion.Trim();
                iquery = iquery.Where(v => v.Solicitud != null && !string.IsNullOrEmpty(v.Solicitud.TipoInstalacion) && EF.Functions.Like(v.Solicitud.TipoInstalacion, $"%{tipo}%"));
            }

            var total = await iquery.CountAsync();

            var items = await iquery
                .OrderByDescending(v => v.FechaVisita)
                .ThenByDescending(v => v.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new EcoInstalacion.Entity.Pagination.PagedResultDto<VisitasTecnica>
            {
                Items = items,
                TotalCount = total,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}
