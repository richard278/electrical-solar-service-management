using EcoInstalacion.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using EcoInstalacion.DAL.EcoInstaladorContext;

namespace EcoInstalacion.DAL.Implementacion
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly EcoInstaladorContext.EcoInstaladorContext _dbContext;

        public GenericRepository(EcoInstaladorContext.EcoInstaladorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TEntity?> ObtenerDatos(Expression<Func<TEntity, bool>> filtro)
        {
            try
            {
                TEntity ? entidad = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(filtro);
                return entidad;
            }
            catch
            {
                throw;
            }
        }

        public async Task<TEntity> Crear(TEntity entidad)
        {
            try
            {
                _dbContext.Set<TEntity>().Add(entidad);
                await _dbContext.SaveChangesAsync();
                return entidad;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(TEntity entidad)
        {
            try
            {
                _dbContext.Update(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(TEntity entidad)
        {
            try
            {
                _dbContext.Remove(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<TEntity>> Consultar(Expression<Func<TEntity, bool>>? filtro = null)
        {
            return filtro == null
                ? await _dbContext.Set<TEntity>().ToListAsync()
                : await _dbContext.Set<TEntity>().Where(filtro).ToListAsync();
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? filtro = null)
        {
            var set = _dbContext.Set<TEntity>().AsQueryable();
            return filtro == null ? set : set.Where(filtro);
        }

        // ====== NUEVO: Consultar con Include(s) ======
        //public async Task<List<TEntity>> ConsultarIncluyendo(
        //    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include,
        //    Expression<Func<TEntity, bool>>? filtro = null)
        //{
        //    IQueryable<TEntity> query = _dbContext.Set<TEntity>();
        //    if (filtro != null) query = query.Where(filtro);
        //    query = include(query);
        //    return await query.ToListAsync();
        //}

        public async Task<List<TEntity>> ConsultarIncluyendo(Func<IQueryable<TEntity>, IQueryable<TEntity>> include, Expression<Func<TEntity, bool>>? filtro = null)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (filtro != null)
                query = query.Where(filtro);

            query = include(query);

            return await query.ToListAsync();
        }
    }
}

/*

🧩 Ventajas de esta versión

✅ Usa Query() para trabajar con IQueryable, evitando ToListAsync() prematuro.
✅ Aplica Includes inteligentes, solo cuando se requieren.
✅ Reduce consumo de memoria y mejora rendimiento en listados grandes.
✅ Devuelve las entidades con relaciones cargadas, listas para la vista.
✅ Mantiene la compatibilidad con tu arquitectura existente (GenericRepository).*/
