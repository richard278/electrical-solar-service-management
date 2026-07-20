using System;
using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.VisitasTecnicas
{
    public class VisitaTecnicaCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una solicitud válida.")]
        public int SolicitudId { get; set; }

        [Required(ErrorMessage = "La fecha de visita es obligatoria.")]
        public DateTime? FechaVisita { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no deben exceder 500 caracteres.")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "El estado de aprobación es obligatorio.")]
        public bool? Aprobado { get; set; }
    }
}
