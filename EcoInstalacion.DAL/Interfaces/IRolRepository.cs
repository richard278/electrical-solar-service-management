using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IRolRepository
    {
        Task<List<Rol>> ListaSinTracking(string? filtro = null, bool? soloActivos = null);
    }
}
