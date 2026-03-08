using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMVisitasTecnicas
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La solicitud es obligatoria.")]
        public int SolicitudId { get; set; }

        [Required(ErrorMessage = "La fecha de visita es obligatoria.")]
        public DateTime FechaVisita { get; set; }
        
        [StringLength(500)]
        public string? Observaciones { get; set; }

        public bool Aprobado { get; set; }
    }
}