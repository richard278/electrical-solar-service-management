using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IClienteRepository
    {
        Task<bool> TieneSolicitudes(int clienteId);
        Task<List<Cliente>> ListaActivosSinTracking();
        Task<Cliente?> ObtenerPorIdSinTracking(int id);
    }
}
