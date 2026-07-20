using System;

namespace EcoInstalacion.AppWeb.DTOs.VisitasTecnicas
{
    public class VisitaTecnicaListDto
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public DateTime FechaVisita { get; set; }
        public string? Observaciones { get; set; }
        public bool Aprobado { get; set; }
        public string? ClienteNombre { get; set; }
        public string? TipoInstalacion { get; set; }
        public string? EstadoSolicitud { get; set; }
        public string? TecnicoNombre { get; set; }
    }
}
