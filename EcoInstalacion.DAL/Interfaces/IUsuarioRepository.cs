using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> ListaConRolSinTracking();
        Task<Usuario?> ObtenerPorIdConRolSinTracking(int idUsuario);
        Task<Usuario?> ObtenerPorIdConRolTracking(int idUsuario);
        Task<Usuario?> ObtenerPorCorreoConRolTracking(string correo);
    }
}
