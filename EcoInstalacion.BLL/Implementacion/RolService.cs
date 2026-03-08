using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.EntityFrameworkCore;

namespace EcoInstalacion.BLL.Implementacion
{
    public class RolService : IRolService
    {
        private readonly IGenericRepository<Rol> _repoRol;
        private readonly IGenericRepository<Usuario> _repoUsuario;

        public RolService(IGenericRepository<Rol> repoRol,
                          IGenericRepository<Usuario> repoUsuario)
        {
            _repoRol = repoRol;
            _repoUsuario = repoUsuario;
        }

        public async Task<List<Rol>> Lista(string? filtro = null, bool? soloActivos = null)
        {
            var q = _repoRol.Query();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var f = filtro.Trim().ToUpper();
                q = q.Where(r => r.Descripcion.ToUpper().Contains(f));
            }

            if (soloActivos is not null)
                q = q.Where(r => r.EsActivo == soloActivos);

            return await q
                .OrderBy(r => r.Descripcion)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Rol?> ObtenerPorId(int idRol)
        {
           return await _repoRol.ObtenerDatos(r => r.IdRol == idRol);
        }

        public async Task<Rol> Crear(Rol entidad)
        {
            if (string.IsNullOrWhiteSpace(entidad.Descripcion))
                throw new ArgumentException("La descripción es obligatoria.", nameof(entidad.Descripcion));

            // Único por nombre (case-insensitive)
            var yaExiste = await _repoRol.ObtenerDatos(r =>
                r.Descripcion.ToUpper() == entidad.Descripcion.Trim().ToUpper());

            if (yaExiste is not null)
                throw new InvalidOperationException("Ya existe un rol con esa descripción.");

            entidad.Descripcion = entidad.Descripcion.Trim();
            entidad.EsActivo = entidad.EsActivo; // respeta lo que venga (o true por defecto)
            entidad.FechaRegistro = DateTime.UtcNow;

            return await _repoRol.Crear(entidad);
        }

        public async Task<Rol> Editar(Rol entidad)
        {
            var db = await _repoRol.ObtenerDatos(r => r.IdRol == entidad.IdRol)
                     ?? throw new KeyNotFoundException("Rol no encontrado.");

            // Si cambia la descripción, validar unicidad
            if (!string.Equals(db.Descripcion, entidad.Descripcion, StringComparison.OrdinalIgnoreCase))
            {
                var colision = await _repoRol.ObtenerDatos(r =>
                    r.IdRol != entidad.IdRol &&
                    r.Descripcion.ToUpper() == entidad.Descripcion.Trim().ToUpper());

                if (colision is not null)
                    throw new InvalidOperationException("Ya existe otro rol con esa descripción.");
            }

            db.Descripcion = entidad.Descripcion.Trim();
            db.EsActivo = entidad.EsActivo;

            await _repoRol.Editar(db);
            return db;
        }

        public async Task<bool> CambiarEstado(int idRol, bool esActivo)
        {
            var db = await _repoRol.ObtenerDatos(r => r.IdRol == idRol)
                     ?? throw new KeyNotFoundException("Rol no encontrado.");
            db.EsActivo = esActivo;
            return await _repoRol.Editar(db);
        }

        public async Task<bool> Eliminar(int idRol)
        {
            var db = await _repoRol.ObtenerDatos(r => r.IdRol == idRol)
                     ?? throw new KeyNotFoundException("Rol no encontrado.");

            // Evitar borrar roles en uso
            var tieneUsuarios = (await _repoUsuario.Consultar(u => u.IdRol == idRol)).Any();
            if (tieneUsuarios)
                throw new InvalidOperationException("No se puede eliminar el rol: tiene usuarios asignados.");

            return await _repoRol.Eliminar(db);
        }
    }
}