using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Auth
{
    public class AuthLoginRequestDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un formato de correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria.")]
        public string Clave { get; set; } = string.Empty;
    }
}
