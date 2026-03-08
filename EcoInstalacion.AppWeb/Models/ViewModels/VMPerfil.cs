using Microsoft.AspNetCore.Http;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMPerfil
    {
        public int IdUsuario { get; set; }
        //Datos visibles
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Rol { get; set; }

        // Imagen actual
        public string? UrlFoto { get; set; }
        public string? NombreFoto { get; set; }
    }

    public class VMPasswordChange
    {
        public int IdUsuario { get; set; }
        public string ClaveActual { get; set; } = null!;
        public string NuevaClave { get; set; } = null!;
        public string ConfirmarClave { get; set; } = null!;
    }
}
