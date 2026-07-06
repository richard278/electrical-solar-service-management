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

        public async Task<Cliente?> ObtenerPorIdSinTracking(int id)
        {
            return await _dbContext.Set<Cliente>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
