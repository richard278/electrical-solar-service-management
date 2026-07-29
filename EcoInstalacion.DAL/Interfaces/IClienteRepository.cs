using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IClienteRepository
    {
        Task<bool> TieneSolicitudes(int clienteId);
        Task<List<Cliente>> ListaActivosSinTracking();
        Task<List<Cliente>> ListaPorEstadoSinTracking(bool? esActivo);
        Task<Cliente?> ObtenerPorIdSinTracking(int id);
        Task<EcoInstalacion.Entity.Pagination.PagedResultDto<Cliente>> ListaPaginadaSinTracking(EcoInstalacion.Entity.Pagination.ClienteQueryDto query, bool? fuerzaActivo = null);
    }
}
