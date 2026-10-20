using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;
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

        public async Task<PagedResult<VisitasTecnica>> ListaPaginadaSinTracking(
            string? q = null,
            bool? aprobado = null,
            int page = 1,
            int pageSize = 25)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedPageSize = pageSize < 1 ? 25 : (pageSize > 100 ? 100 : pageSize);

            var query = _dbContext.Set<VisitasTecnica>()
                .AsNoTracking()
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .Include(v => v.Solicitud)
                    .ThenInclude(s => s.TecnicoAsignado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(v =>
                    v.Solicitud != null &&
                    v.Solicitud.Cliente != null &&
                    v.Solicitud.Cliente.NombreCompleto.Contains(term));
            }

            if (aprobado.HasValue)
            {
                query = aprobado.Value
                    ? query.Where(v => v.Aprobado == true)
                    : query.Where(v => v.Aprobado == false || v.Aprobado == null);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(v => v.FechaVisita)
                .ThenByDescending(v => v.Id)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync();

            return new PagedResult<VisitasTecnica>(
                items,
                normalizedPage,
                normalizedPageSize,
                totalCount);
        }
    }
}
