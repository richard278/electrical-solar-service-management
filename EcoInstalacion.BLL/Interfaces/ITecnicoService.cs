using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface ITecnicoService
    {
        Task<List<Tecnico>> Lista();
        Task<Tecnico?> ObtenerPorId(int id);
        Task<Tecnico> Crear(Tecnico entidad);
        Task<Tecnico> Editar(Tecnico entidad);
        Task<bool> Eliminar(int id);
        Task<EcoInstalacion.Entity.Pagination.PagedResultDto<Tecnico>> ListaPaginada(EcoInstalacion.Entity.Pagination.TecnicoQueryDto query);
    }
}
