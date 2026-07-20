using EcoInstalacion.Entity;

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
    }
}
