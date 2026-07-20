using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Solicitudes
{
    public class SolicitudCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El tipo de instalación es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El tipo de instalación debe tener entre 3 y 50 caracteres.")]
        public string? TipoInstalacion { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no debe exceder 500 caracteres.")]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un técnico válido.")]
        public int? TecnicoAsignadoId { get; set; }
    }
}
