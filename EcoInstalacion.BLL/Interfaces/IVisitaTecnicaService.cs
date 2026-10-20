using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

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
        Task<PagedResult<VisitasTecnica>> ListaPaginada(
            string? q = null,
            bool? aprobado = null,
            int page = 1,
            int pageSize = 25);
    }
}
