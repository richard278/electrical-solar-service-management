namespace EcoInstalacion.Entity;

public partial class Cliente
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Dui { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public string? Correo { get; set; }

    public bool EsActivo { get; set; } = true;

    public virtual ICollection<SolicitudesInstalacion> SolicitudesInstalacions { get; set; } = new List<SolicitudesInstalacion>();
}