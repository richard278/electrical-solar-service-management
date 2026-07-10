using System;

namespace EcoInstalacion.AppWeb.DTOs.VisitasTecnicas
{
    public class VisitaTecnicaListDto
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public DateTime FechaVisita { get; set; }
        public string? Observaciones { get; set; }
        public bool? Aprobado { get; set; }
    }
}
