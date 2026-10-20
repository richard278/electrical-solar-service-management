using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface ITecnicoService
    {
        Task<List<Tecnico>> Lista();
        Task<Tecnico?> ObtenerPorId(int id);
        Task<Tecnico> Crear(Tecnico entidad);
        Task<Tecnico> Editar(Tecnico entidad);
        Task<bool> Eliminar(int id);
        Task<PagedResult<Tecnico>> ListaPaginada(
            string? q = null,
            int page = 1,
            int pageSize = 25);
    }
}
