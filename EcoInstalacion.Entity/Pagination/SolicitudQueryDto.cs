using System;

namespace EcoInstalacion.Entity.Pagination
{
    public class SolicitudQueryDto : PagedRequestDto
    {
        public string? q { get; set; }
        public string? estado { get; set; }
        public int? clienteId { get; set; }
        public int? tecnicoAsignadoId { get; set; }
        public string? tipoInstalacion { get; set; }
        public DateTime? fechaDesde { get; set; }
        public DateTime? fechaHasta { get; set; }
        public string? clienteNombre { get; set; }
        public string? tecnicoNombre { get; set; }
    }
}
