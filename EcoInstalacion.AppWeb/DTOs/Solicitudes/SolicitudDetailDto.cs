using System;

namespace EcoInstalacion.AppWeb.DTOs.Solicitudes
{
    public class SolicitudDetailDto
    {
        public int Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? TipoInstalacion { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public int? TecnicoAsignadoId { get; set; }
        public string? TecnicoNombre { get; set; }
    }
}
