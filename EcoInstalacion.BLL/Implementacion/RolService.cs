using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Seguridad;

namespace EcoInstalacion.BLL.Implementacion
{
    public class RolService : IRolService
    {
        private readonly IGenericRepository<Rol> _repoRol;
        private readonly IRolRepository _rolRepository;

        public RolService(IGenericRepository<Rol> repoRol,
                          IRolRepository rolRepository)
        {
            _repoRol = repoRol;
            _rolRepository = rolRepository;
        }

        public async Task<List<Rol>> Lista(string? filtro = null, bool? soloActivos = null)
        {
            var roles = await _rolRepository.ListaSinTracking(filtro, soloActivos);

            return roles
                .Where(RolesSistema.EsRolOperativo)
                .OrderBy(r => r.IdRol)
                .ToList();
        }

        public async Task<Rol?> ObtenerPorId(int idRol)
        {
            if (!RolesSistema.EsIdOperativo(idRol))
                return null;

            var rol = await _repoRol.ObtenerDatos(r => r.IdRol == idRol);
            return RolesSistema.EsRolOperativo(rol) ? rol : null;
        }

        public Task<Rol> Crear(Rol entidad)
        {
            ArgumentNullException.ThrowIfNull(entidad);
            throw CatalogoCerrado();
        }

        public Task<Rol> Editar(Rol entidad)
        {
            ArgumentNullException.ThrowIfNull(entidad);
            throw CatalogoCerrado();
        }

        public Task<bool> CambiarEstado(int idRol, bool esActivo)
        {
            throw CatalogoCerrado();
        }

        public Task<bool> Eliminar(int idRol)
        {
            throw CatalogoCerrado();
        }

        private static InvalidOperationException CatalogoCerrado() =>
            new("El catálogo de roles es cerrado y de solo lectura. Los únicos roles permitidos son Administrador y Técnico.");
    }
}
