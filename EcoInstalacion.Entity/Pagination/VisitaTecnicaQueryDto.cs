using System;

namespace EcoInstalacion.Entity.Pagination
{
    public class VisitaTecnicaQueryDto : PagedRequestDto
    {
        public string? q { get; set; }
        public bool? aprobado { get; set; }
        public int? solicitudId { get; set; }
        public string? estadoSolicitud { get; set; }
        public string? clienteNombre { get; set; }
        public string? tecnicoNombre { get; set; }
        public string? tipoInstalacion { get; set; }
        public DateTime? fechaDesde { get; set; }
        public DateTime? fechaHasta { get; set; }
    }
}
