using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IClienteRepository
    {
        Task<bool> TieneSolicitudes(int clienteId);
        Task<List<Cliente>> ListaActivosSinTracking();
        Task<List<Cliente>> ListaPorEstadoSinTracking(bool? esActivo);
        Task<Cliente?> ObtenerPorIdSinTracking(int id);
        Task<PagedResult<Cliente>> ListaPaginadaSinTracking(
            string? q = null,
            bool? esActivo = null,
            int page = 1,
            int pageSize = 25);
    }
}
