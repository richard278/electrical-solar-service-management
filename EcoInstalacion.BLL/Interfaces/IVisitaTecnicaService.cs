using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IVisitaTecnicaService
    {
        Task<List<VisitasTecnica>> Lista();                     // Todas, con Include a Solicitud(+Cliente)
        Task<List<VisitasTecnica>> ListaPorSolicitud(int solicitudId);
        Task<VisitasTecnica?> ObtenerPorId(int id);
        Task<VisitasTecnica> Crear(VisitasTecnica entidad);
        Task<VisitasTecnica> Editar(VisitasTecnica entidad);
        Task<bool> Eliminar(int id);
        Task<EcoInstalacion.Entity.Pagination.PagedResultDto<VisitasTecnica>> ListaPaginada(
            EcoInstalacion.Entity.Pagination.VisitaTecnicaQueryDto query);
    }
}
