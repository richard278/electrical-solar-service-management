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
    }
}
