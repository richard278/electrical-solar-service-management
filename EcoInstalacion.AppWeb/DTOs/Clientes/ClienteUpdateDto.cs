using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Clientes
{
    public class ClienteUpdateDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre completo no debe exceder 150 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DUI es obligatorio.")]
        [StringLength(20, ErrorMessage = "El DUI no debe exceder 20 caracteres.")]
        public string Dui { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "El teléfono no debe exceder 30 caracteres.")]
        public string? Telefono { get; set; }

        [StringLength(250, ErrorMessage = "La dirección no debe exceder 250 caracteres.")]
        public string? Direccion { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no debe exceder 150 caracteres.")]
        public string? Correo { get; set; }

        public bool EsActivo { get; set; } = true;
    }
}
