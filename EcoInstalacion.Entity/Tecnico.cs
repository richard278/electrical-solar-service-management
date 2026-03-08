using System;
using System.Collections.Generic;

namespace EcoInstalacion.Entity;

public partial class Tecnico
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Especialidad { get; set; }

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public virtual ICollection<SolicitudesInstalacion> SolicitudesInstalacions { get; set; } = new List<SolicitudesInstalacion>();
}