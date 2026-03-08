using System.IO;
using System.Threading.Tasks;

namespace EcoInstalacion.BLL.Interfaces
{
    /// <summary>
    /// Define las operaciones estándar para el manejo local de imágenes de usuario.
    /// Se usa principalmente en el contexto de EcoInstalacion.AppWeb para guardar,
    /// recuperar y eliminar imágenes asociadas a los usuarios.
    /// </summary>
    public interface ILocalImageStorage
    {
        ///// <summary>
        ///// Guarda una imagen en el sistema de archivos local de la aplicación (wwwroot/uploads/users).
        ///// </summary>
        ///// <param name="fileStream">Stream de la imagen a guardar.</param>
        ///// <param name="fileName">Nombre único del archivo (se recomienda incluir extensión).</param>
        ///// <returns>Nombre del archivo almacenado, o null si la operación falla.</returns>
        Task<string?> SaveUserImageAsync(Stream fileStream, string fileName);
        ///// <summary>
        ///// Elimina una imagen existente del almacenamiento local.
        ///// </summary>
        ///// <param name="fileName">Nombre del archivo a eliminar (sin la ruta completa).</param>
        ///// <returns>True si se eliminó correctamente, false si no se encontró o falló.</returns>
        Task<bool> DeleteUserImageAsync(string fileName);
        ///// <summary>
        ///// Devuelve la URL relativa accesible públicamente de una imagen de usuario.
        ///// </summary>
        ///// <param name="fileName">Nombre del archivo (opcional).</param>
        ///// <returns>Cadena con la URL relativa (por ejemplo: "/uploads/users/foto.png"), o la ruta por defecto si no existe.</returns>
        string GetUserImageUrl(string? fileName);
    }
}