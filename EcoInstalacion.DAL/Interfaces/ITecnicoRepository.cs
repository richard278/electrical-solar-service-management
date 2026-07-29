using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface ITecnicoRepository
    {
        Task<bool> EstaAsignadoASolicitudes(int tecnicoId);
        Task<List<Tecnico>> ListaSinTracking();
        Task<Tecnico?> ObtenerPorIdSinTracking(int id);
        Task<EcoInstalacion.Entity.Pagination.PagedResultDto<Tecnico>> ListaPaginadaSinTracking(EcoInstalacion.Entity.Pagination.TecnicoQueryDto query);
    }
}
