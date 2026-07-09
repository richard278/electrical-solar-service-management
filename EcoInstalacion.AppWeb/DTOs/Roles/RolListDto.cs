namespace EcoInstalacion.AppWeb.DTOs.Roles
{
    public class RolListDto
    {
        public int IdRol { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool EsActivo { get; set; }
    }
}
