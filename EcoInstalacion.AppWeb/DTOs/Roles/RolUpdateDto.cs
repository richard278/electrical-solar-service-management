using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.DTOs.Roles
{
    public class RolUpdateDto
    {
        [Required(ErrorMessage = "La descripción del rol es obligatoria.")]
        [StringLength(30, ErrorMessage = "La descripción no debe exceder 30 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public bool EsActivo { get; set; } = true;
    }
}
