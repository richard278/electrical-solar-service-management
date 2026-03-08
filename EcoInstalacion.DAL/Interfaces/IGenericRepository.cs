using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> ObtenerDatos(Expression<Func<TEntity, bool>> filtro);
        Task<TEntity> Crear(TEntity entidad);
        Task<bool> Editar(TEntity entidad);
        Task<bool> Eliminar(TEntity entidad);
        Task<List<TEntity>> Consultar(Expression<Func<TEntity, bool>>? filtro = null);
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? filtro = null);

        // NUEVO: consultar con Include(s)
        Task<List<TEntity>> ConsultarIncluyendo(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include,
            Expression<Func<TEntity, bool>>? filtro = null);

    }
}
