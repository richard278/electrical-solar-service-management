namespace EcoInstalacion.Entity.Pagination
{
    public class ClienteQueryDto : PagedRequestDto
    {
        public string? q { get; set; }
        public string? nombre { get; set; }
        public string? correo { get; set; }
        public string? dui { get; set; }
        public string? telefono { get; set; }
        public string? estado { get; set; }
    }
}
