using System.ComponentModel.DataAnnotations;
namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMLogin
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = string.Empty;

        public bool Recordarme { get; set; } = false;

        public string? ReturnUrl { get; set; }


    }
}
