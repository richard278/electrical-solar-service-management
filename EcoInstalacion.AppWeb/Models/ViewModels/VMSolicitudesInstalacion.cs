using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMSolicitudesInstalacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de solicitud es obligatoria.")]
        [Display(Name = "Fecha de solicitud")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaSolicitud { get; set; }   // ← nullable + Required

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int? ClienteId { get; set; }             // ← nullable + Required
        public string? ClienteNombre { get; set; }      // solo lectura

        [Required(ErrorMessage = "El tipo de instalación es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo de instalación no debe superar los 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El tipo de instalación debe tener al menos 3 caracteres.")]
        [Display(Name = "Tipo de instalación")]
        public string? TipoInstalacion { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no debe superar los 500 caracteres.")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20, ErrorMessage = "El estado no debe superar los 20 caracteres.")]
        public string? Estado { get; set; }

        [Display(Name = "Técnico asignado")]
        public int? TecnicoAsignadoId { get; set; }     // opcional
        public string? TecnicoNombre { get; set; }      // solo lectura
    }
}
