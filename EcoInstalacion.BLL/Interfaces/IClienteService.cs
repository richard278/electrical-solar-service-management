using EcoInstalacion.Entity;

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
        Task<EcoInstalacion.Entity.Pagination.PagedResultDto<Cliente>> ListaPaginada(EcoInstalacion.Entity.Pagination.ClienteQueryDto query, bool esAdmin = false);
    }
}
