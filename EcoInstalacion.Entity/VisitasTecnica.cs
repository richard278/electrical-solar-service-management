using System;
using System.Collections.Generic;

namespace EcoInstalacion.Entity;

public partial class VisitasTecnica
{
    public int Id { get; set; }

    public int SolicitudId { get; set; }

    public DateTime FechaVisita { get; set; }

    public string? Observaciones { get; set; }

    public bool? Aprobado { get; set; }

    public virtual SolicitudesInstalacion Solicitud { get; set; } = null!;
}
