using EcoInstalacion.BLL.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.IO;

namespace EcoInstalacion.AppWeb.Utilidades.Storage
{
    /*Servicio de almacenamiento local de imágenes de usuario.
    Administra guardado, borrado y obtención de rutas públicas
    en la carpeta wwwroot/uploads/users.*/
    
    public class LocalImageStore : ILocalImageStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _folderPath;
        private const string RelativeFolder = "uploads/users";

        public LocalImageStore(IWebHostEnvironment env)
        {
            _env = env;
            _folderPath = Path.Combine(_env.WebRootPath, "uploads", "users");

            // 🧱 Asegura que la carpeta exista
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
        }

        /// <summary>
        /// Guarda una imagen localmente en wwwroot/uploads/users con nombre único.
        /// </summary>
        /// <param name="fileStream">Stream de imagen recibido desde el formulario.</param>
        /// <param name="fileName">Nombre del archivo original (usado solo para extensión).</param>
        /// <returns>Nombre final del archivo almacenado o null si falla.</returns>
        public async Task<string?> SaveUserImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                if (fileStream == null || string.IsNullOrWhiteSpace(fileName))
                    return null;

                // 🔒 Validar extensión
                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                var validExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!validExt.Contains(ext))
                    throw new InvalidOperationException("Formato de imagen no permitido.");

                // 📏 Validar tamaño máximo (2 MB)
                if (fileStream.Length > 2 * 1024 * 1024)
                    throw new InvalidOperationException("La imagen excede el tamaño máximo permitido (2 MB).");

                // 🧩 Generar nombre seguro y ruta completa
                var safeName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(_folderPath, safeName);

                // 💾 Guardar imagen
                using (var outStream = File.Create(fullPath))
                    await fileStream.CopyToAsync(outStream);

                return safeName;
            }
            catch
            {
                return null;
            }
        }

        //// <summary>
        //// Elimina una imagen de usuario del almacenamiento local.
        //// </summary>
        //// <param name="fileName">Nombre del archivo a eliminar.</param>
        //// <returns>True si se eliminó correctamente o si no existía.</returns>
        public async Task<bool> DeleteUserImageAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return true;
            var fullPath = Path.Combine(_env.WebRootPath, RelativeFolder, fileName);
            if (!File.Exists(fullPath)) return true;
            try { File.Delete(fullPath); return await Task.FromResult(true); }
            catch { return false; }
        }

        /// <summary>
        /// Retorna la URL pública de una imagen o una predeterminada si no existe.
        /// </summary>
        /// <param name="fileName">Nombre de archivo.</param>
        /// <returns>Ruta relativa accesible vía HTTP.</returns>
        public string GetUserImageUrl(string? fileName)
        {
            return string.IsNullOrWhiteSpace(fileName)
                ? $"/{RelativeFolder}/default.png"
                : $"/{RelativeFolder}/{fileName}";
        }
    }
}

//💡 Detalles técnicos destacados:

//✔️ Cumple 100 % con la interfaz ILocalImageStorage

//⚙️ Guarda en wwwroot/uploads/users

//🧩 Valida extensión y tamaño máximo

//💾 Usa Guid como nombre único para evitar colisiones

//🧱 Crea carpeta automáticamente si no existe

//🚫 Evita excepción en caso de borrado fallido (“best effort”)

//🌐 Devuelve ruta accesible desde la vista (por ejemplo, /uploads/users/imagen123.png)