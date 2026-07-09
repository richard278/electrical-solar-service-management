namespace EcoInstalacion.AppWeb.DTOs.Clientes
{
    public class ClienteDetailDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Dui { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Correo { get; set; }
        public bool EsActivo { get; set; }
    }
}
