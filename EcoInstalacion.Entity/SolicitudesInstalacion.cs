namespace EcoInstalacion.Entity;

public class SolicitudesInstalacion
{
    public int Id { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int ClienteId { get; set; }
    public string? TipoInstalacion { get; set; }
    public string? Descripcion { get; set; }
    public string? Estado { get; set; }
    public int? TecnicoAsignadoId { get; set; }
    public virtual Cliente? Cliente { get; set; }
    public virtual Tecnico? TecnicoAsignado { get; set; } // Ensure this property exists
    public virtual ICollection<VisitasTecnica>? VisitasTecnicas { get; set; }
}
