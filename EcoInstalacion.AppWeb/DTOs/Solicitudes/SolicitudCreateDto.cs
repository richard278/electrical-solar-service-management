using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Solicitudes
{
    public class SolicitudCreateDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int ClienteId { get; set; }

        [StringLength(50, ErrorMessage = "El tipo de instalación no debe exceder 50 caracteres.")]
        public string? TipoInstalacion { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no debe exceder 500 caracteres.")]
        public string? Descripcion { get; set; }

        [StringLength(20, ErrorMessage = "El estado no debe exceder 20 caracteres.")]
        public string? Estado { get; set; }

        public int? TecnicoAsignadoId { get; set; }
    }
}
