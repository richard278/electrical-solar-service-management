using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Tecnicos
{
    public class TecnicoCreateDto
    {
        [Required(ErrorMessage = "El nombre del técnico es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "La especialidad no debe exceder 50 caracteres.")]
        public string? Especialidad { get; set; }

        [StringLength(15, ErrorMessage = "El teléfono no debe exceder 15 caracteres.")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "El correo no debe exceder 100 caracteres.")]
        public string? Correo { get; set; }
    }
}
