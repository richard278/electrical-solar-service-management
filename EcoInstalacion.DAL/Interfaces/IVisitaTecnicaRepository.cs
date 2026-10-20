using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IVisitaTecnicaRepository
    {
        Task<List<VisitasTecnica>> ListaConSolicitudCliente();
        Task<List<VisitasTecnica>> ListaPorSolicitudConSolicitudCliente(int solicitudId);
        Task<VisitasTecnica?> ObtenerPorIdConSolicitudCliente(int id);
        Task<bool> EditarConTransicionAprobacion(
            VisitasTecnica visita,
            bool actualizarSolicitudAEnProceso);
        Task<PagedResult<VisitasTecnica>> ListaPaginadaSinTracking(
            string? q = null,
            bool? aprobado = null,
            int page = 1,
            int pageSize = 25);
    }
}
