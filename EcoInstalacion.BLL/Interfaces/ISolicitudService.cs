using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface ISolicitudService
    {
        Task<List<SolicitudesInstalacion>> Lista();
        Task<SolicitudesInstalacion?> ObtenerPorId(int id);
        Task<SolicitudesInstalacion> Crear(SolicitudesInstalacion entidad);
        Task<SolicitudesInstalacion> Editar(SolicitudesInstalacion entidad);
        Task<bool> Eliminar(int id);
        Task<PagedResult<SolicitudesInstalacion>> ListaPaginada(
            string? q = null,
            string? estado = null,
            int page = 1,
            int pageSize = 25);
    }
}
