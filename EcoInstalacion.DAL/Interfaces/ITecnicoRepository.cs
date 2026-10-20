using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface ITecnicoRepository
    {
        Task<bool> EstaAsignadoASolicitudes(int tecnicoId);
        Task<List<Tecnico>> ListaSinTracking();
        Task<Tecnico?> ObtenerPorIdSinTracking(int id);
        Task<PagedResult<Tecnico>> ListaPaginadaSinTracking(
            string? q = null,
            int page = 1,
            int pageSize = 25);
    }
}
