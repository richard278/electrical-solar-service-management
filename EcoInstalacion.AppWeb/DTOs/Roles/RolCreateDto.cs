using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Roles
{
    public class RolCreateDto
    {
        [Required(ErrorMessage = "La descripción del rol es obligatoria.")]
        [StringLength(100, ErrorMessage = "La descripción no debe exceder 100 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public bool EsActivo { get; set; } = true;
    }
}
