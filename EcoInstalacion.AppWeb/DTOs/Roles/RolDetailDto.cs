namespace EcoInstalacion.AppWeb.DTOs.Roles
{
    public class RolDetailDto
    {
        public int IdRol { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool EsActivo { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }
}
