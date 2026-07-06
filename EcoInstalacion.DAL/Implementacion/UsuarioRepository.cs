using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.EntityFrameworkCore;

namespace EcoInstalacion.DAL.Implementacion
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly EcoInstalacion.DAL.EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public UsuarioRepository(EcoInstalacion.DAL.EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Usuario>> ListaConRolSinTracking()
        {
            return await _dbContext.Set<Usuario>()
                .AsNoTracking()
                .Include(u => u.IdRolNavigation)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdConRolSinTracking(int idUsuario)
        {
            return await _dbContext.Set<Usuario>()
                .AsNoTracking()
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        public async Task<Usuario?> ObtenerPorIdConRolTracking(int idUsuario)
        {
            return await _dbContext.Set<Usuario>()
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        public async Task<Usuario?> ObtenerPorCorreoConRolTracking(string correo)
        {
            return await _dbContext.Set<Usuario>()
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }
    }
}
