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
    }
}
