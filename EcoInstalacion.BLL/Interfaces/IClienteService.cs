using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IClienteService
    {
        Task<List<Cliente>> Lista();
        Task<List<Cliente>> ListaPorEstado(bool? esActivo);
        Task<Cliente?> ObtenerPorId(int id);
        Task<Cliente> Crear(Cliente entidad);
        Task<Cliente> Editar(Cliente entidad);
        Task<bool> Eliminar(int id);
        Task<PagedResult<Cliente>> ListaPaginada(
            string? q = null,
            bool? esActivo = null,
            int page = 1,
            int pageSize = 25);
    }
}
