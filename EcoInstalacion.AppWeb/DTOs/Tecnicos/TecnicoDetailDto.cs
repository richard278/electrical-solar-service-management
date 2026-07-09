namespace EcoInstalacion.AppWeb.DTOs.Tecnicos
{
    public class TecnicoDetailDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }
}
