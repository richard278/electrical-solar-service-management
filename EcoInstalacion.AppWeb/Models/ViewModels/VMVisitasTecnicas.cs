using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMVisitasTecnicas
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una solicitud válida.")]
        public int SolicitudId { get; set; }

        [Required(ErrorMessage = "La fecha de visita es obligatoria.")]
        public DateTime FechaVisita { get; set; }
        
        [StringLength(500, ErrorMessage = "Las observaciones no deben exceder 500 caracteres.")]
        public string? Observaciones { get; set; }

        public bool Aprobado { get; set; }
    }
}
