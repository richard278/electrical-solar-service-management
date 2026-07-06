using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.EntityFrameworkCore;
using EcoInstalacion.DAL.EcoInstaladorContext;

namespace EcoInstalacion.DAL.Implementacion
{
    public class RolRepository : IRolRepository
    {
        private readonly EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public RolRepository(EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Rol>> ListaSinTracking(string? filtro = null, bool? soloActivos = null)
        {
            var query = _dbContext.Set<Rol>()
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var f = filtro.Trim().ToUpper();

                query = query.Where(r =>
                    r.Descripcion != null &&
                    r.Descripcion.ToUpper().Contains(f));
            }

            if (soloActivos is not null)
            {
                query = query.Where(r => r.EsActivo == soloActivos);
            }

            return await query
                .OrderBy(r => r.Descripcion)
                .ToListAsync();
        }
    }
}
