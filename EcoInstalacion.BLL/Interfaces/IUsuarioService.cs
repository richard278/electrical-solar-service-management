using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IUsuarioService
    {
        // 📜 Listar todos los usuarios con sus roles
        Task<List<Usuario>> Lista();

        // 📜 Crear nuevo usuario (con posibilidad de incluir foto local o remota)
        Task<Usuario> Crear(Usuario entidad, Stream? foto = null, string NombreFoto = "", string urlPlantillaCorreo = "");

        // ✏️ Editar usuario existente (datos y/o imagen)
        Task<Usuario> Editar(Usuario entidad, Stream? foto = null, string NombreFoto = "");

        // ❌ Eliminar usuario (y su foto local si aplica)
        Task<bool> Eliminar(int idUsuario);

        // 🔑 Validar credenciales (login)
        Task<Usuario> ObtenerPorCredenciales(string correo, string clave);

        // 🔍 Buscar por Id
        Task<Usuario> ObtenerPorId(int idUsuario);

        // 🔄 Cambiar contraseña autenticada
        Task<bool> CambiarClave(int idUsuario, string claveActual, string nuevaClave);

        // 📬 Restablecer contraseña (envío de correo con plantilla)
        Task<bool> RestablecerClave(string correo, string urlPlantillaCorreo);

        // 🧩 Actualizar datos de perfil (correo, teléfono, imagen)
        Task<bool> GuardarPerfil(Usuario entidad);
    }
}
