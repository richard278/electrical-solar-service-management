using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Clientes
{
    public class ClienteCreateDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre completo no debe exceder 100 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DUI es obligatorio.")]
        [StringLength(10, ErrorMessage = "El DUI no debe exceder 10 caracteres.")]
        public string Dui { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "El teléfono no debe exceder 15 caracteres.")]
        public string? Telefono { get; set; }

        [StringLength(150, ErrorMessage = "La dirección no debe exceder 150 caracteres.")]
        public string? Direccion { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "El correo no debe exceder 100 caracteres.")]
        public string? Correo { get; set; }
    }
}
