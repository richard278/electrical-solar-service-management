using System;

namespace EcoInstalacion.AppWeb.DTOs.Reportes
{
    public class ReporteTecnicoAsignadoDto
    {
        public string Tecnico { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaVisita { get; set; }
        public string TipoInstalacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public bool Aprobado { get; set; }
    }
}
