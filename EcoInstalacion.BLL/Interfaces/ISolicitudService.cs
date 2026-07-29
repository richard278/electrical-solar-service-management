using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface ISolicitudService
    {
        Task<List<SolicitudesInstalacion>> Lista();
        Task<SolicitudesInstalacion?> ObtenerPorId(int id);
        Task<SolicitudesInstalacion> Crear(SolicitudesInstalacion entidad);
        Task<SolicitudesInstalacion> Editar(SolicitudesInstalacion entidad);
        Task<bool> Eliminar(int id);
        Task<EcoInstalacion.Entity.Pagination.PagedResultDto<SolicitudesInstalacion>> ListaPaginada(EcoInstalacion.Entity.Pagination.SolicitudQueryDto query);
    }
}
