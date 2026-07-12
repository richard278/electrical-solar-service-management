namespace EcoInstalacion.AppWeb.DTOs.Auth
{
    public class AuthUserDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
    }
}
